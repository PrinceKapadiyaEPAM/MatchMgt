namespace Inventory.Web.Controllers;

using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Infrastructure;
using Inventory.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;

    public InventoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [RequirePermission(AppModules.Inventory, "View")]
    public async Task<IActionResult> Index(string? search, TransactionType? type, int? catalogueId, int page = 1, int pageSize = 10)
    {
        var query = _db.InventoryTransactions
            .Include(x => x.Catalogue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Catalogue.Name.Contains(search));
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        if (catalogueId.HasValue)
        {
            query = query.Where(t => t.CatalogueId == catalogueId.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Type = type;
        ViewBag.CatalogueId = catalogueId;
        ViewBag.PageSize = pageSize;
        ViewBag.Catalogues = _db.Catalogues.ToList();

        var model = new Domain.ViewModels.PaginatedList<InventoryTransaction>(items, total, page, pageSize);

        return View(model);
    }

    // CREATE GET
    [RequirePermission(AppModules.Inventory, "View")]
    public IActionResult Create()
    {
        ViewBag.Catalogues = _db.Catalogues.ToList();
        return View();
    }

    // CREATE POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Inventory, "Edit")]
    public async Task<IActionResult> Create(InventoryTransaction model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Catalogues = _db.Catalogues.ToList();
            return View(model);
        }

        model.TransactionDate = DateTime.UtcNow;

        _db.InventoryTransactions.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Transaction saved.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    [RequirePermission(AppModules.Inventory, "View")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.InventoryTransactions
            .Include(x => x.Catalogue)
            .FirstOrDefaultAsync(x => x.Id == id);

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Inventory, "Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.InventoryTransactions.FindAsync(id);

        _db.InventoryTransactions.Remove(item);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}