namespace Inventory.Web.Controllers;
using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Inventory.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class CatalogueController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CatalogueController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    #region Index
    [RequirePermission(AppModules.Catalogue, "View")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        // build base query
        var query = _db.Catalogues
            .Select(c => new CatalogueStockVM
            {
                Id = c.Id,
                Name = c.Name,
                Price = c.Price,
                Remark = c.Remark,

                Stock = _db.InventoryTransactions
                    .Where(t => t.CatalogueId == c.Id)
                    .Sum(t => t.TransactionType == TransactionType.Stock
                            ? t.Quantity
                            : -t.Quantity)
                    - _db.DispatchEntries
                        .Where(d => d.CatalogueId == c.Id && (d.Status == "Ok" || d.Status == "Pending"))
                        .Sum(d => (int?)d.Bale ?? 0)
            });

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.Like(x.Name, $"%{search}%"));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PaginatedList<CatalogueStockVM>(items, total, page, pageSize);

        ViewBag.Search = search;
        ViewBag.PageSize = pageSize;

        return View(model);
    }
    #endregion


    #region AddEdit
    [RequirePermission(AppModules.Catalogue, "View")]
    public async Task<IActionResult> AddEdit(int? id)
    {
        ViewBag.Action = "Create";
        ViewBag.AllCategories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        if (id != null)
        {
            ViewBag.Action = "Edit";
            var item = await _db.Catalogues.FindAsync(id);

            if (item == null)
                return NotFound();

            ViewBag.SelectedCategoryIds = await _db.CatalogueCategories
                .Where(cc => cc.CatalogueId == id)
                .Select(cc => cc.CategoryId)
                .ToListAsync();

            return View("AddEdit", item);
        }

        ViewBag.SelectedCategoryIds = new List<int>();
        return View("AddEdit", new Catalogue());
    }
    #endregion


    #region Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Catalogue, "Edit")]
    public async Task<IActionResult> Save(Catalogue model, IFormFile? photo, IFormFile? pdf, int[]? selectedCategoryIds)
    {
        if (!ModelState.IsValid)
            return View("Edit", model);

        #region Upload Photo & PDF
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
        Directory.CreateDirectory(uploads);

        // PHOTO
        if (photo != null && photo.Length > 0)
        {
            var fileName = $"photo_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(filePath);
            await photo.CopyToAsync(fs);

            model.PhotoFileName = fileName;
        }

        // PDF
        if (pdf != null && pdf.Length > 0)
        {
            var fileName = $"doc_{Guid.NewGuid()}{Path.GetExtension(pdf.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(filePath);
            await pdf.CopyToAsync(fs);

            model.PdfFileName = fileName;
        }
        #endregion

        if (model.Id == 0)
        {
            _db.Catalogues.Add(model);
            TempData["Success"] = "Product created successfully.";
        }
        else
        {
            var item = await _db.Catalogues.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Fold = model.Fold;
            item.Price = model.Price;
            item.Remark = model.Remark;
            item.RestockDate = model.RestockDate;

            if (!string.IsNullOrEmpty(model.PhotoFileName))
                item.PhotoFileName = model.PhotoFileName;

            if (!string.IsNullOrEmpty(model.PdfFileName))
                item.PdfFileName = model.PdfFileName;

            TempData["Success"] = "Product updated successfully.";
        }

        await _db.SaveChangesAsync();

        // sync category assignments (full-replace); model.Id is populated by EF after SaveChangesAsync
        var existing = await _db.CatalogueCategories.Where(cc => cc.CatalogueId == model.Id).ToListAsync();
        _db.CatalogueCategories.RemoveRange(existing);

        if (selectedCategoryIds?.Length > 0)
        {
            _db.CatalogueCategories.AddRange(selectedCategoryIds.Select(cid => new CatalogueCategory
            {
                CatalogueId = model.Id,
                CategoryId = cid
            }));
        }

        await _db.SaveChangesAsync();

        if (model.Price == null)
            TempData["PriceWarning"] = "No price set — this item will show 'Price on Request' to mobile app users.";

        return RedirectToAction(nameof(Index));
    }
    #endregion


    [HttpPost]
    [RequirePermission(AppModules.Catalogue, "Edit")]
    public async Task<IActionResult> AddStock([FromBody] AddStockRequest req)
    {
        try
        {
            var entry = new InventoryTransaction
            {
                CatalogueId     = req.CatalogueId,
                TransactionType = TransactionType.Stock,
                Quantity        = req.Quantity,
                TransactionDate = DateTime.SpecifyKind(DateTime.Parse(req.Date), DateTimeKind.Utc)
            };
            _db.InventoryTransactions.Add(entry);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [RequirePermission(AppModules.Catalogue, "View")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Catalogue, "Edit")]
    public async Task<IActionResult> Create(Catalogue model, IFormFile? photo, IFormFile? pdf)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (photo != null && photo.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"photo_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(fs);
            }
            model.PhotoFileName = fileName;
        }

        if (pdf != null && pdf.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(pdf.FileName);
            var fileName = $"doc_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await pdf.CopyToAsync(fs);
            }
            model.PdfFileName = fileName;
        }

        _db.Catalogues.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Product created successfully.";
        if (model.Price == null)
            TempData["PriceWarning"] = "No price set — this item will show 'Price on Request' to mobile app users.";
        return RedirectToAction(nameof(Index));
    }


    [RequirePermission(AppModules.Catalogue, "View")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);
        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Catalogue, "Edit")]
    public async Task<IActionResult> Edit(int id, Catalogue model, IFormFile? photo, IFormFile? pdf)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var item = await _db.Catalogues.FindAsync(id);
        if (item == null) return NotFound();

        // update scalar properties
        item.Name = model.Name;
        item.Remark = model.Remark;
        item.RestockDate = model.RestockDate;

        if (photo != null && photo.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"photo_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(fs);
            }
            item.PhotoFileName = fileName;
        }

        if (pdf != null && pdf.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(pdf.FileName);
            var fileName = $"doc_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await pdf.CopyToAsync(fs);
            }
            item.PdfFileName = fileName;
        }

        _db.Catalogues.Update(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully.";
        if (model.Price == null)
            TempData["PriceWarning"] = "No price set — this item will show 'Price on Request' to mobile app users.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission(AppModules.Catalogue, "View")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);
        if (item == null) return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Catalogue, "Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);

        if (item != null)
        {
            _db.Catalogues.Remove(item);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

public record AddStockRequest(int CatalogueId, int Quantity, string Date);
