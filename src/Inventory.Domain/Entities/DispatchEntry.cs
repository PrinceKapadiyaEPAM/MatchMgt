using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Domain.Entities;

[Table("DispatchEntries")]
public class DispatchEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int? CatalogueId { get; set; }
    [ValidateNever]
    public Catalogue? Catalogue { get; set; }
    public string? Fold { get; set; }
    public string? Agent { get; set; }
    public int? ToPartyId { get; set; }
    [ValidateNever]
    public Party? ToParty { get; set; }
    public string? From { get; set; }
    public string? Transport { get; set; }
    public string? Station { get; set; }
    public int? Bale { get; set; }
    public string? Remark { get; set; }
    public string? BaleNo { get; set; }
    public decimal? Price { get; set; }
    public string Status { get; set; } = "Pending";
}
