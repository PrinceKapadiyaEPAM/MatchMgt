using Inventory.Infrastructure;
using Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public class PartyPricesController : Controller
{
    private readonly ApplicationDbContext _db;

    public PartyPricesController(ApplicationDbContext db) => _db = db;

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(int partyId)
    {
        var party = await _db.Party.AsNoTracking().FirstOrDefaultAsync(p => p.PartyId == partyId);
        if (party == null) return NotFound();

        var catalogues = await _db.Catalogues
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var overrides = await _db.CataloguePartyPrices
            .AsNoTracking()
            .Where(p => p.PartyId == partyId)
            .ToDictionaryAsync(p => p.CatalogueId, p => p.OverridePrice);

        ViewBag.Party = party;
        ViewBag.Overrides = overrides;
        return View(catalogues);
    }
    #endregion

    #region Save
    public record PriceItem(int CatalogueId, decimal? OverridePrice);
    public record SavePartyPricesRequest(int PartyId, List<PriceItem> Prices);

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SavePartyPricesRequest req)
    {
        try
        {
            var partyExists = await _db.Party.AnyAsync(p => p.PartyId == req.PartyId);
            if (!partyExists) return Json(new { success = false, error = "Party not found." });

            foreach (var item in req.Prices)
            {
                if (item.OverridePrice.HasValue && item.OverridePrice.Value <= 0)
                    return Json(new { success = false, error = $"Override price for catalogue {item.CatalogueId} must be greater than zero." });
            }

            var existingRows = await _db.CataloguePartyPrices
                .Where(p => p.PartyId == req.PartyId)
                .ToListAsync();

            var existingDict = existingRows.ToDictionary(p => p.CatalogueId);

            foreach (var item in req.Prices)
            {
                if (item.OverridePrice.HasValue)
                {
                    if (existingDict.TryGetValue(item.CatalogueId, out var existing))
                    {
                        existing.OverridePrice = item.OverridePrice.Value;
                        existing.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                    }
                    else
                    {
                        _db.CataloguePartyPrices.Add(new CataloguePartyPrice
                        {
                            CatalogueId = item.CatalogueId,
                            PartyId = req.PartyId,
                            OverridePrice = item.OverridePrice.Value,
                            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                        });
                    }
                }
                else
                {
                    if (existingDict.TryGetValue(item.CatalogueId, out var toDelete))
                        _db.CataloguePartyPrices.Remove(toDelete);
                }
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
    #endregion
}
