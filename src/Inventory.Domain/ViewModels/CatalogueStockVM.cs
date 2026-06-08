namespace Inventory.Domain.ViewModels;

public class CatalogueStockVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public string Remark { get; set; }
    public int Stock { get; set; }
}