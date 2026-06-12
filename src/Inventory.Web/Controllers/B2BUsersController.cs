using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Inventory.Web.Filters;
using Inventory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[Authorize]
public class B2BUsersController : Controller
{
    private readonly ApplicationDbContext _db;

    public B2BUsersController(ApplicationDbContext db) => _db = db;

    #region Index
    [HttpGet]
    [RequirePermission(AppModules.B2BUsers, "View")]
    public async Task<IActionResult> Index(string? search, string? approvalStatus, int page = 1, int pageSize = 20)
    {
        var query = _db.B2BUsers.AsNoTracking().Include(u => u.Party).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => EF.Functions.ILike(u.FullName, $"%{search}%") ||
                                     EF.Functions.ILike(u.Email, $"%{search}%"));

        if (!string.IsNullOrWhiteSpace(approvalStatus))
            query = query.Where(u => u.ApprovalStatus == approvalStatus);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.ApprovalStatus = approvalStatus;
        return View(new PaginatedList<B2BUser>(items, totalCount, page, pageSize));
    }
    #endregion

    #region AddEdit
    [HttpGet]
    [RequirePermission(AppModules.B2BUsers, "View")]
    public async Task<IActionResult> AddEdit(int? id)
    {
        var vm = new B2BUserEditVM
        {
            Parties = await _db.Party.AsNoTracking().OrderBy(p => p.PartyName).ToListAsync()
        };

        if (id.HasValue)
        {
            var user = await _db.B2BUsers.FindAsync(id.Value);
            if (user == null) return NotFound();
            vm.Id = user.Id;
            vm.FullName = user.FullName;
            vm.Email = user.Email;
            vm.Phone = user.Phone;
            vm.PartyId = user.PartyId;
            vm.IsActive = user.IsActive;
        }

        ViewBag.Action = id.HasValue ? "Edit" : "Create";
        return View(vm);
    }
    #endregion

    #region Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.B2BUsers, "Edit")]
    public async Task<IActionResult> Save(B2BUserEditVM model)
    {
        model.Parties = await _db.Party.AsNoTracking().OrderBy(p => p.PartyName).ToListAsync();

        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError("", "Full Name and Email are required.");
            ViewBag.Action = model.Id == 0 ? "Create" : "Edit";
            return View("AddEdit", model);
        }

        if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("Password", "Password is required when creating a new user.");
            ViewBag.Action = "Create";
            return View("AddEdit", model);
        }

        var emailTaken = await _db.B2BUsers
            .AnyAsync(u => u.Email == model.Email.Trim().ToLowerInvariant() && u.Id != model.Id);
        if (emailTaken)
        {
            ModelState.AddModelError("Email", "This email address is already registered.");
            ViewBag.Action = model.Id == 0 ? "Create" : "Edit";
            return View("AddEdit", model);
        }

        var hasher = new PasswordHasher<B2BUser>();

        if (model.Id == 0)
        {
            var user = new B2BUser
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Phone = model.Phone?.Trim(),
                PartyId = model.PartyId,
                IsActive = true,
                ApprovalStatus = "Approved",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            user.PasswordHash = hasher.HashPassword(user, model.Password!);
            _db.B2BUsers.Add(user);
        }
        else
        {
            var user = await _db.B2BUsers.FindAsync(model.Id);
            if (user == null) return NotFound();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim().ToLowerInvariant();
            user.Phone = model.Phone?.Trim();
            user.PartyId = model.PartyId;
            user.IsActive = model.IsActive;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.PasswordHash = hasher.HashPassword(user, model.Password);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "B2B user saved successfully.";
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Approve
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.B2BUsers, "Edit")]
    public async Task<IActionResult> Approve(int id, int? partyId)
    {
        var user = await _db.B2BUsers.FindAsync(id);
        if (user == null) return NotFound();
        user.ApprovalStatus = "Approved";
        user.IsActive = true;
        if (partyId.HasValue) user.PartyId = partyId;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{user.FullName} has been approved.";
        return RedirectToAction(nameof(Index), new { approvalStatus = "Pending" });
    }
    #endregion

    #region Reject
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.B2BUsers, "Edit")]
    public async Task<IActionResult> Reject(int id)
    {
        var user = await _db.B2BUsers.FindAsync(id);
        if (user == null) return NotFound();
        user.ApprovalStatus = "Rejected";
        user.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{user.FullName} has been rejected.";
        return RedirectToAction(nameof(Index), new { approvalStatus = "Pending" });
    }
    #endregion

    #region ToggleActive
    [HttpPost]
    [RequirePermission(AppModules.B2BUsers, "Edit")]
    public async Task<IActionResult> ToggleActive([FromBody] int id)
    {
        var user = await _db.B2BUsers.FindAsync(id);
        if (user == null) return Json(new { success = false, error = "User not found." });
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return Json(new { success = true, isActive = user.IsActive });
    }
    #endregion

    #region AccessRights
    [HttpGet]
    [RequirePermission(AppModules.B2BUsers, "View")]
    public async Task<IActionResult> AccessRights(int id)
    {
        var user = await _db.B2BUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.B2BUsers, "Edit")]
    public async Task<IActionResult> SaveAccessRights(int id, bool showCatalogue, bool showPrices, bool showStock)
    {
        var user = await _db.B2BUsers.FindAsync(id);
        if (user == null) return NotFound();
        user.ShowCatalogue = showCatalogue;
        user.ShowPrices    = showPrices;
        user.ShowStock     = showStock;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Access rights updated for {user.FullName}.";
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    [HttpGet]
    [RequirePermission(AppModules.B2BUsers, "View")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.B2BUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.B2BUsers, "Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _db.B2BUsers.FindAsync(id);
        if (user != null) _db.B2BUsers.Remove(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "B2B user deleted.";
        return RedirectToAction(nameof(Index));
    }
    #endregion
}
