using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Inventory.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[Authorize]
public class DispatchController : Controller
{
    private readonly ApplicationDbContext _db;

    public DispatchController(ApplicationDbContext db) => _db = db;

    [RequirePermission(AppModules.Dispatch, "View")]
    public async Task<IActionResult> Index(
        string? search,
        string? status,
        int?    partyId,
        int?    catalogueId,
        string? dateFrom,
        string? dateTo,
        string  sortBy   = "date",
        string  sortDir  = "desc",
        int     page     = 1,
        int     pageSize = 20)
    {
        var query = _db.DispatchEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                EF.Functions.ILike(x.Agent     ?? "", $"%{search}%") ||
                EF.Functions.ILike(x.Station   ?? "", $"%{search}%") ||
                EF.Functions.ILike(x.BaleNo    ?? "", $"%{search}%") ||
                EF.Functions.ILike(x.Transport ?? "", $"%{search}%"));

        if (!string.IsNullOrEmpty(status))                           query = query.Where(x => x.Status == status);
        if (partyId.HasValue)                                        query = query.Where(x => x.ToPartyId == partyId);
        if (catalogueId.HasValue)                                    query = query.Where(x => x.CatalogueId == catalogueId);
        if (DateOnly.TryParse(dateFrom, out var df))                 query = query.Where(x => x.Date >= df);
        if (DateOnly.TryParse(dateTo,   out var dt))                 query = query.Where(x => x.Date <= dt);

        query = (sortBy, sortDir) switch
        {
            ("date",   "asc")  => query.OrderBy(x => x.Date).ThenBy(x => x.Id),
            ("status", "asc")  => query.OrderBy(x => x.Status),
            ("status", _)      => query.OrderByDescending(x => x.Status),
            ("qty",    "asc")  => query.OrderBy(x => x.Qty),
            ("qty",    _)      => query.OrderByDescending(x => x.Qty),
            _                  => query.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id),
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CatalogueList = await _db.Catalogues
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.FoldList = await _db.Catalogues
            .Where(x => !x.IsDeleted && x.Fold != null)
            .Select(x => x.Fold)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        ViewBag.PartyList = await _db.Party
            .OrderBy(x => x.PartyName)
            .ToListAsync();

        ViewBag.AgentList = await _db.DispatchEntries
            .Where(x => x.Agent != null)
            .Select(x => x.Agent)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        ViewBag.TransportList = await _db.DispatchEntries
            .Where(x => x.Transport != null)
            .Select(x => x.Transport)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        ViewBag.Search       = search;
        ViewBag.StatusFilter = status;
        ViewBag.PartyFilter  = partyId;
        ViewBag.CatFilter    = catalogueId;
        ViewBag.DateFrom     = dateFrom;
        ViewBag.DateTo       = dateTo;
        ViewBag.SortBy       = sortBy;
        ViewBag.SortDir      = sortDir;
        ViewBag.PageSize     = pageSize;

        return View(new PaginatedList<DispatchEntry>(items, total, page, pageSize));
    }

    [RequirePermission(AppModules.Dispatch, "View")]
    public async Task<IActionResult> AddDispatch(string? date)
    {
        var target  = DateOnly.TryParse(date, out var d) ? d : DateOnly.FromDateTime(DateTime.Today);
        var entries = await _db.DispatchEntries
            .Where(x => x.Date == target)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        ViewBag.TargetDate = target.ToString("yyyy-MM-dd");

        ViewBag.CatalogueList = await _db.Catalogues
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.FoldList = await _db.Catalogues
            .Where(x => !x.IsDeleted && x.Fold != null)
            .Select(x => x.Fold)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        ViewBag.PartyList = await _db.Party
            .OrderBy(x => x.PartyName)
            .ToListAsync();

        ViewBag.AgentList = await _db.DispatchEntries
            .Where(x => x.Agent != null)
            .Select(x => x.Agent)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        ViewBag.TransportList = await _db.DispatchEntries
            .Where(x => x.Transport != null)
            .Select(x => x.Transport)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return View(entries);
    }

    [HttpPost]
    [RequirePermission(AppModules.Dispatch, "Edit")]
    public async Task<IActionResult> SaveRow([FromBody] DispatchEntry model)
    {
        try
        {
            if (model.Id == 0)
            {
                await _db.DispatchEntries.AddAsync(model);
            }
            else
            {
                var entry = await _db.DispatchEntries.FindAsync(model.Id);
                if (entry == null) return Json(new { success = false, error = "Record not found." });

                entry.Date = model.Date;
                entry.CatalogueId = model.CatalogueId;
                entry.Fold = model.Fold;
                entry.Agent = model.Agent;
                entry.ToPartyId = model.ToPartyId;
                entry.From = model.From;
                entry.Transport = model.Transport;
                entry.Station = model.Station;
                entry.Bale = model.Bale;
                entry.Remark = model.Remark;
                entry.Qty = model.Qty;
                entry.BaleNo = model.BaleNo;
                entry.Price = model.Price;
                entry.Status = model.Status;
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, id = model.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [RequirePermission(AppModules.Dispatch, "Delete")]
    public async Task<IActionResult> DeleteRow([FromBody] int id)
    {
        var entry = await _db.DispatchEntries.FindAsync(id);
        if (entry == null) return Json(new { success = false });

        _db.DispatchEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }
}
