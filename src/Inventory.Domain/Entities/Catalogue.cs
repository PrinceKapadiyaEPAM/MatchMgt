namespace Inventory.Domain.Entities;

public class Catalogue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Fold { get; set; }
    public decimal? Price { get; set; }
    public string? Remark { get; set; }
    public string? PhotoFileName { get; set; }
    public string? PdfFileName { get; set; }
    public bool IsDeleted { get; set; }
    public DateOnly? RestockDate { get; set; }

    public ICollection<InventoryTransaction> Transactions { get; set; }
        = new List<InventoryTransaction>();

    public ICollection<CatalogueCategory> CatalogueCategories { get; set; } = new List<CatalogueCategory>();
}