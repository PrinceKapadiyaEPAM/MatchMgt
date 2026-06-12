using Inventory.Domain.Entities;

namespace Inventory.Web.Helpers;

public static class StockStatusHelper
{
    public static (int stockQty, string stockStatus) Compute(
        int catalogueId,
        IQueryable<InventoryTransaction> transactions,
        IQueryable<DispatchEntry> dispatches,
        DateOnly? restockDate,
        DateOnly today)
    {
        var stockIn = transactions
            .Where(t => t.CatalogueId == catalogueId && t.TransactionType == TransactionType.Stock)
            .Sum(t => (int?)t.Quantity) ?? 0;
        var stockOut = transactions
            .Where(t => t.CatalogueId == catalogueId && t.TransactionType == TransactionType.Order)
            .Sum(t => (int?)t.Quantity) ?? 0;
        var dispatched = dispatches
            .Where(d => d.CatalogueId == catalogueId && (d.Status == "Ok" || d.Status == "Pending"))
            .Sum(d => (int?)d.Bale) ?? 0;

        var raw = stockIn - stockOut - dispatched;
        var capped = Math.Max(raw, 0);

        string status;
        if (raw > 0)
            status = "In Stock";
        else if (restockDate.HasValue && restockDate.Value > today)
            status = $"Coming Soon \u2014 {restockDate.Value.DayNumber - today.DayNumber} day(s)";
        else
            status = "Out of Stock";

        return (capped, status);
    }
}
