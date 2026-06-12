using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Domain.Entities;

[Table("CataloguePartyPrices")]
public class CataloguePartyPrice
{
    public int Id { get; set; }
    public int CatalogueId { get; set; }
    [ValidateNever] public Catalogue Catalogue { get; set; } = null!;
    public int PartyId { get; set; }
    [ValidateNever] public Party Party { get; set; } = null!;
    public decimal OverridePrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
