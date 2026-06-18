using Inventory.Domain.Entities;
using Inventory.Infrastructure;
using Inventory.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[ApiController]
[Route("api/b2b/catalogue")]
// NOTE: intentionally not decorated with [Authorize] so this controller supports
// both anonymous and authenticated (B2B) requests. Authentication middleware
// may optionally populate HttpContext.Items["B2BUser"].
public class B2BCatalogueController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public B2BCatalogueController(ApplicationDbContext db) => _db = db;

    // Try to resolve the currently authenticated B2B user if present in HttpContext.Items
    private async Task<B2BUser?> TryGetCurrentUserAsync()
    {
        // First check if already in Items (from prior auth)
        if (HttpContext?.Items != null && HttpContext.Items.TryGetValue("B2BUser", out var obj) && obj is B2BUser bu)
            return bu;

        // If not already authenticated, manually trigger B2B authentication
        if (HttpContext != null)
        {
            var result = await HttpContext.AuthenticateAsync("B2B");
            if (result.Succeeded && result.Principal != null)
            {
                // The OnTokenValidated event should have populated HttpContext.Items["B2BUser"]
                if (HttpContext.Items.TryGetValue("B2BUser", out var authenticatedUser) && authenticatedUser is B2BUser bbu)
                    return bbu;
            }
        }

        return null;
    }

    private decimal? ResolvePrice(Dictionary<int, decimal> partyPrices, int catalogueId, decimal? defaultPrice)
    {
        if (partyPrices.TryGetValue(catalogueId, out var overridePrice))
            return overridePrice;
        return defaultPrice;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? categoryId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var user = await TryGetCurrentUserAsync();

        // If an authenticated user exists and they are not allowed to view catalogue, forbid.
        if (user != null && !user.ShowCatalogue)
            return Forbid();

        // For anonymous callers, allow catalogue listing but do not expose party-specific behavior.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.Catalogues
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CatalogueCategories.Any(cc => cc.CategoryId == categoryId.Value));
        }

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

        // If an authenticated party user exists, load their overrides; otherwise use empty map.
        var partyPrices = user?.PartyId.HasValue == true
            ? await _db.CataloguePartyPrices
                .AsNoTracking()
                .Where(p => p.PartyId == user!.PartyId!.Value && catalogueIds.Contains(p.CatalogueId))
                .ToDictionaryAsync(p => p.CatalogueId, p => p.OverridePrice)
            : new Dictionary<int, decimal>();

        var showPrices = user?.ShowPrices ?? true;
        var showStock = user?.ShowStock ?? true;

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
                // If an authenticated user exists with party prices, resolve override; otherwise return catalogue price.
                price = showPrices ? ResolvePrice(partyPrices, c.Id, c.Price) : (decimal?)null,
                stockQty = showStock ? (int?)stockQty : null,
                stockStatus = showStock ? stockStatus : null
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
        var user = await TryGetCurrentUserAsync();

        if (user != null && !user.ShowCatalogue)
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

        // Resolve price: if authenticated user with party override, use that; otherwise use catalogue price.
        decimal? price = c.Price;
        if (user?.PartyId.HasValue == true)
        {
            var overridePrice = await _db.CataloguePartyPrices
                .AsNoTracking()
                .Where(p => p.PartyId == user.PartyId.Value && p.CatalogueId == c.Id)
                .Select(p => (decimal?)p.OverridePrice)
                .FirstOrDefaultAsync();
            if (overridePrice.HasValue) price = overridePrice;
        }

        var showPrices = user?.ShowPrices ?? true;
        var showStock = user?.ShowStock ?? true;

        return Ok(new
        {
            c.Id,
            c.Name,
            c.Fold,
            c.Remark,
            photoUrl = c.PhotoFileName != null ? $"/uploads/catalogues/{c.PhotoFileName}" : null,
            pdfUrl = c.PdfFileName != null ? $"/uploads/catalogues/{c.PdfFileName}" : null,
            price = showPrices ? price : null,
            stockQty = showStock ? (int?)stockQty : null,
            stockStatus = showStock ? stockStatus : null,
            restockDate = showStock ? c.RestockDate?.ToString("yyyy-MM-dd") : null
        });
    }
}
