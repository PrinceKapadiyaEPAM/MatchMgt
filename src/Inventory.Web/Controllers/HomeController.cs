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

        var stockOutSum = await _db.InventoryTransactions
            .Where(s => s.Quantity > 0 && s.TransactionType == Domain.Entities.TransactionType.Order)
            .SumAsync(s => s.Quantity) ;
        ViewBag.StockOut = stockOutSum;
        var Available = stockIn - stockOutSum;
            ViewBag.StockIn = stockIn;
        ViewBag.Available = Available;
        return View();
    }
}

