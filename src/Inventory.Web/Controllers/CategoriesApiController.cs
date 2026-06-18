using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesApiController(ApplicationDbContext db) => _db = db;

    // GET: /api/categories
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search = null, [FromQuery] int? limit = 0)
    {
        var query = _db.Categories.AsNoTracking().Where(c => true);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));

        query = query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);
        if (limit > 0)
            query = query.Take((int)limit);

        var items = await query.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            imageUrl = c.ImageFileName != null ? $"/uploads/categories/{c.ImageFileName}" : null,
            catalogueCount = c.CatalogueCategories.Count
        }).ToListAsync();
        return Ok(items);
    }
}
