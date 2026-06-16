namespace Inventory.Domain.Entities;

public class CatalogueCategory
{
    public int CatalogueId { get; set; }
    public int CategoryId { get; set; }

    public Catalogue Catalogue { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
