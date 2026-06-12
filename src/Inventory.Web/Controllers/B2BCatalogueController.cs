using Inventory.Infrastructure;
using Inventory.Domain.Entities;
using Inventory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[ApiController]
[Route("api/b2b/catalogue")]
[Authorize(AuthenticationSchemes = "B2B")]
public class B2BCatalogueController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public B2BCatalogueController(ApplicationDbContext db) => _db = db;

    private B2BUser CurrentUser => (B2BUser)HttpContext.Items["B2BUser"]!;

    private decimal? ResolvePrice(Dictionary<int, decimal> partyPrices, int catalogueId, decimal? defaultPrice)
    {
        if (partyPrices.TryGetValue(catalogueId, out var overridePrice))
            return overridePrice;
        return defaultPrice;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        pageSize = Math.Min(pageSize, 100);
        var user = CurrentUser;

        if (!user.ShowCatalogue)
            return Forbid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.Catalogues
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));

        var totalCount = await query.CountAsync();
        var catalogues = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var catalogueIds = catalogues.Select(c => c.Id).ToList();

        // Batch-load stock aggregates for all page items
        var txSums = await _db.InventoryTransactions
            .AsNoTracking()
            .Where(t => catalogueIds.Contains(t.CatalogueId))
            .GroupBy(t => new { t.CatalogueId, t.TransactionType })
            .Select(g => new { g.Key.CatalogueId, g.Key.TransactionType, Total = g.Sum(t => t.Quantity) })
            .ToListAsync();

        var dispatchSums = await _db.DispatchEntries
            .AsNoTracking()
            .Where(d => d.CatalogueId.HasValue && catalogueIds.Contains(d.CatalogueId!.Value) && (d.Status == "Ok" || d.Status == "Pending"))
            .GroupBy(d => d.CatalogueId!.Value)
            .Select(g => new { CatalogueId = g.Key, Total = g.Sum(d => d.Bale ?? 0) })
            .ToDictionaryAsync(g => g.CatalogueId, g => g.Total);

        var stockInMap = txSums
            .Where(t => t.TransactionType == TransactionType.Stock)
            .ToDictionary(t => t.CatalogueId, t => t.Total);
        var stockOutMap = txSums
            .Where(t => t.TransactionType == TransactionType.Order)
            .ToDictionary(t => t.CatalogueId, t => t.Total);

        // Batch-load party price overrides for the current user
        var partyPrices = user.PartyId.HasValue
            ? await _db.CataloguePartyPrices
                .AsNoTracking()
                .Where(p => p.PartyId == user.PartyId.Value && catalogueIds.Contains(p.CatalogueId))
                .ToDictionaryAsync(p => p.CatalogueId, p => p.OverridePrice)
            : new Dictionary<int, decimal>();

        var items = catalogues.Select(c =>
        {
            var stockIn = stockInMap.GetValueOrDefault(c.Id);
            var stockOut = stockOutMap.GetValueOrDefault(c.Id);
            var dispatched = dispatchSums.GetValueOrDefault(c.Id);
            var raw = stockIn - stockOut - dispatched;
            var stockQty = Math.Max(raw, 0);
            string stockStatus = raw > 0 ? "In Stock"
                : (c.RestockDate.HasValue && c.RestockDate.Value > today)
                    ? $"Coming Soon — {c.RestockDate.Value.DayNumber - today.DayNumber} day(s)"
                    : "Out of Stock";

            return new
            {
                c.Id,
                c.Name,
                c.Fold,
                photoUrl = c.PhotoFileName != null ? $"/uploads/catalogues/{c.PhotoFileName}" : null,
                price = user.ShowPrices ? ResolvePrice(partyPrices, c.Id, c.Price) : (decimal?)null,
                stockQty  = user.ShowStock ? (int?)stockQty : null,
                stockStatus = user.ShowStock ? stockStatus : null
            };
        }).ToList();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            items
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var user = CurrentUser;

        if (!user.ShowCatalogue)
            return Forbid();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var c = await _db.Catalogues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (c == null) return NotFound(new { error = "Catalogue item not found." });

        var (stockQty, stockStatus) = StockStatusHelper.Compute(
            c.Id,
            _db.InventoryTransactions.AsNoTracking(),
            _db.DispatchEntries.AsNoTracking(),
            c.RestockDate,
            today);

        decimal? price = c.Price;
        if (user.PartyId.HasValue)
        {
            var overridePrice = await _db.CataloguePartyPrices
                .AsNoTracking()
                .Where(p => p.PartyId == user.PartyId.Value && p.CatalogueId == c.Id)
                .Select(p => (decimal?)p.OverridePrice)
                .FirstOrDefaultAsync();
            if (overridePrice.HasValue) price = overridePrice;
        }

        return Ok(new
        {
            c.Id,
            c.Name,
            c.Fold,
            c.Remark,
            photoUrl = c.PhotoFileName != null ? $"/uploads/catalogues/{c.PhotoFileName}" : null,
            pdfUrl = c.PdfFileName != null ? $"/uploads/catalogues/{c.PdfFileName}" : null,
            price     = user.ShowPrices ? price : null,
            stockQty  = user.ShowStock  ? (int?)stockQty : null,
            stockStatus = user.ShowStock ? stockStatus : null,
            restockDate = user.ShowStock ? c.RestockDate?.ToString("yyyy-MM-dd") : null
        });
    }
}
