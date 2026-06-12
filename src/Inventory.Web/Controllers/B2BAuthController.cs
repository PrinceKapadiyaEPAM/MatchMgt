using Inventory.Infrastructure;
using Inventory.Domain.Entities;
using Inventory.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[ApiController]
[Route("api/b2b/auth")]
public class B2BAuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly B2BJwtService _jwt;

    public B2BAuthController(ApplicationDbContext db, B2BJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword, string? Phone);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email and password are required." });

        var normalizedEmail = req.Email.Trim().ToLowerInvariant();
        var user = await _db.B2BUsers
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null)
            return Unauthorized(new { error = "Invalid email or password." });

        var hasher = new PasswordHasher<B2BUser>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { error = "Invalid email or password." });

        if (user.ApprovalStatus == "Pending")
            return Unauthorized(new { error = "Your account is pending approval. Please contact your administrator." });

        if (!user.IsActive || user.ApprovalStatus == "Rejected")
            return Unauthorized(new { error = "Your account has been deactivated. Please contact your administrator." });

        user.LastLoginAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _jwt.GenerateToken(user.Id);
        return Ok(new
        {
            token,
            expiresAt,
            userType = user.PartyId.HasValue ? "PartyUser" : "GuestUser",
            fullName = user.FullName
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.ConfirmPassword))
            return BadRequest(new { error = "All required fields must be provided." });

        if (req.Password != req.ConfirmPassword)
            return BadRequest(new { error = "Passwords do not match." });

        if (req.Password.Length < 8)
            return BadRequest(new { error = "Password must be at least 8 characters." });

        var emailExists = await _db.B2BUsers
            .AnyAsync(u => u.Email == req.Email.Trim().ToLowerInvariant());
        if (emailExists)
            return BadRequest(new { error = "This email address is already registered." });

        var hasher = new PasswordHasher<B2BUser>();
        var user = new B2BUser
        {
            FullName = req.FullName.Trim(),
            Email = req.Email.Trim().ToLowerInvariant(),
            Phone = req.Phone?.Trim(),
            IsActive = false,
            ApprovalStatus = "Pending",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };
        user.PasswordHash = hasher.HashPassword(user, req.Password);

        _db.B2BUsers.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registration successful. Your account is pending approval. Please contact your administrator." });
    }
}
