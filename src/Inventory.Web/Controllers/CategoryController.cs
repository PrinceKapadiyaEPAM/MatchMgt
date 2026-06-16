namespace Inventory.Web.Controllers;
using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Inventory.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CategoryController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    #region Index
    [RequirePermission(AppModules.Category, "View")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var query = _db.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.PageSize = pageSize;

        return View(new PaginatedList<Category>(items, total, page, pageSize));
    }
    #endregion


    #region AddEdit
    [RequirePermission(AppModules.Category, "View")]
    public async Task<IActionResult> AddEdit(int? id)
    {
        ViewBag.Action = "Create";

        if (id != null)
        {
            ViewBag.Action = "Edit";
            var item = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        return View(new Category());
    }
    #endregion


    #region Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Category, "Edit")]
    public async Task<IActionResult> Save(Category model, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Action = model.Id == 0 ? "Create" : "Edit";
            return View("AddEdit", model);
        }

        if (image != null && image.Length > 0)
        {
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                ModelState.AddModelError("image", "Only JPG and PNG images are allowed.");
                ViewBag.Action = model.Id == 0 ? "Create" : "Edit";
                return View("AddEdit", model);
            }
            if (image.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError("image", "Image must be 3 MB or smaller.");
                ViewBag.Action = model.Id == 0 ? "Create" : "Edit";
                return View("AddEdit", model);
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads", "categories");
            Directory.CreateDirectory(uploads);
            var fileName = $"image_{Guid.NewGuid()}{ext}";
            using var fs = System.IO.File.Create(Path.Combine(uploads, fileName));
            await image.CopyToAsync(fs);
            model.ImageFileName = fileName;
        }

        if (model.Id == 0)
        {
            _db.Categories.Add(model);
            TempData["Success"] = "Category created successfully.";
        }
        else
        {
            var item = await _db.Categories.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.SortOrder = model.SortOrder;

            if (!string.IsNullOrEmpty(model.ImageFileName))
                item.ImageFileName = model.ImageFileName;

            TempData["Success"] = "Category updated successfully.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    #endregion


    #region Delete
    [RequirePermission(AppModules.Category, "View")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission(AppModules.Category, "Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Categories.FindAsync(id);
        if (item != null)
        {
            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Category deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
    #endregion
}
