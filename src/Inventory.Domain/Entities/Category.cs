namespace Inventory.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? ImageFileName { get; set; }

    public ICollection<CatalogueCategory> CatalogueCategories { get; set; } = new List<CatalogueCategory>();
}
