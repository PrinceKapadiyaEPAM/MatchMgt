using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Inventory.Web.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PublicController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET: /api/public/info
    [HttpGet("info")]
    public async Task<IActionResult> Info()
    {
        var profile = await _db.CompanyProfile.AsNoTracking().FirstOrDefaultAsync();

        string? logoUrl = null;
        if (profile?.LogoFileName != null && Request != null)
        {
            var scheme = Request.Scheme;
            var host = Request.Host.Value;
            logoUrl = $"{scheme}://{host}/uploads/company/{profile.LogoFileName}";
        }

        var response = new
        {
            success = true,
            data = new
            {
                company = new
                {
                    name = profile?.Name,
                    tagline = profile?.Tagline,
                    logoUrl,
                    brandMarkText = profile?.BrandmarkText
                },
                contact = new
                {
                    phone = profile?.Phone,
                    mobile = profile?.Mobile,
                    email = profile?.Email,
                    addressLine1 = profile?.Address,
                    city = profile?.City,
                    country = (string?)null,
                    postalCode = profile?.Pincode
                },
                socialLinks = new object[] { },
                hero = new
                {
                    eyebrow = "Inventory Experience",
                    title = "Find the right catalogue in seconds",
                    subtitle = "Explore categories and products with a modern storefront experience.",
                    imageUrl = (string?)null,
                    primaryCta = new { label = "Browse Categories", path = "/categories" },
                    secondaryCta = new { label = "View Products", path = "/products" }
                },
                footer = new
                {
                    aboutTitle = "Why People Like Us!",
                    aboutText = "Reliable quality, fair pricing, and smooth ordering.",
                    newsletter = new { enabled = true, placeholder = "Your Email", buttonText = "Subscribe Now" },
                    shopLinks = new[] {
                        new { label = "About Us", path = "/about" },
                        new { label = "Contact Us", path = "/contact" },
                        new { label = "Privacy Policy", path = "/privacy" },
                        new { label = "Terms & Conditions", path = "/terms" }
                    },
                    accountLinks = new[] {
                        new { label = "My Account", path = "/account" },
                        new { label = "Order History", path = "/orders" },
                        new { label = "Wishlist", path = "/wishlist" }
                    },
                    payments = new[] { "VISA", "MasterCard", "PayPal" },
                    copyrightText = profile != null ? $"© {profile.Name} {DateTime.UtcNow.Year}, All rights reserved." : null
                }
            }
        };

        return Ok(response);
    }

    // GET: /api/public/media?path=uploads/catalogues/photo_abc.jpg
    // Serve media files (catalogues, companies, etc.) with path traversal protection
    [HttpGet("media")]
    public IActionResult GetMedia([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { error = "Path parameter is required." });

        // Sanitize: remove any .. or leading slashes to prevent directory traversal
        var sanitized = Path.GetFileName(path);
        if (string.IsNullOrEmpty(sanitized))
            return BadRequest(new { error = "Invalid path format." });

        // Determine category from path hint (e.g., "uploads/catalogues/..." → catalogues)
        var category = ExtractCategory(path);
        var webRootPath = _env.WebRootPath;
        var fullPath = Path.Combine(webRootPath, category, sanitized);

        // Ensure resolved path is within webroot to block traversal attacks
        var resolvedPath = Path.GetFullPath(fullPath);
        var webRootFull = Path.GetFullPath(webRootPath);
        if (!resolvedPath.StartsWith(webRootFull, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        // Check file exists
        if (!System.IO.File.Exists(resolvedPath))
            return NotFound(new { error = "File not found." });

        // Determine MIME type
        var mimeType = GetMimeType(resolvedPath);

        // Serve file
        var fileBytes = System.IO.File.ReadAllBytes(resolvedPath);
        return File(fileBytes, mimeType, enableRangeProcessing: true);
    }

    private string ExtractCategory(string path)
    {
        // Extract first directory from path (e.g., "uploads/catalogues/photo.jpg" → "uploads/catalogues")
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return string.Join("/", parts.Take(2));
        return "uploads";
    }

    private string GetMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}
