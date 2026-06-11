namespace Inventory.Web.Controllers;

using Inventory.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class HomeController(ApplicationDbContext db) : Controller
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IActionResult> IndexAsync()
    {
        ViewBag.TotalCatalogues = await _db.Catalogues.CountAsync();

      var stockIn   = await _db.InventoryTransactions
            .Where(s => s.Quantity > 0 && s.TransactionType == Domain.Entities.TransactionType.Stock)
            .SumAsync(s => s.Quantity) ;

        var stockOutSum = await _db.DispatchEntries
            .Where(d => d.Status != "Cancelled" && d.Qty.HasValue)
            .SumAsync(d => d.Qty ?? 0);
        ViewBag.StockOut = stockOutSum;

        ViewBag.TotalBales = await _db.DispatchEntries
            .Where(d => d.Status != "Cancelled" && d.Bale.HasValue)
            .SumAsync(d => d.Bale ?? 0);
        var Available = stockIn - stockOutSum;
            ViewBag.StockIn = stockIn;
        ViewBag.Available = Available;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ChartData(string range = "thisweek")
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly start, end;

        switch (range)
        {
            case "lastweek":
                var daysToLastMonday = ((int)today.DayOfWeek + 6) % 7 + 7;
                start = today.AddDays(-daysToLastMonday);
                end   = start.AddDays(6);
                break;
            case "thismonth":
                start = new DateOnly(today.Year, today.Month, 1);
                end   = today;
                break;
            default:
                var daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
                start = today.AddDays(-daysFromMonday);
                end   = start.AddDays(6);
                break;
        }

        var raw = await _db.DispatchEntries
            .Where(d => d.Date >= start && d.Date <= end
                     && d.Status != "Cancelled" && d.Qty.HasValue)
            .GroupBy(d => d.Date)
            .Select(g => new { Date = g.Key, Qty = g.Sum(x => x.Qty ?? 0) })
            .ToListAsync();

        int days = end.DayNumber - start.DayNumber + 1;
        var result = Enumerable.Range(0, days).Select(i => {
            var d   = start.AddDays(i);
            var row = raw.FirstOrDefault(x => x.Date == d);
            return new { label = d.ToString("ddd dd/MM"), qty = row?.Qty ?? 0 };
        });

        return Json(result);
    }
}

