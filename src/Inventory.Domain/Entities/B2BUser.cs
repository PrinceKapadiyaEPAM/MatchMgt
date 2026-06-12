using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Domain.Entities;

[Table("B2BUsers")]
public class B2BUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? PartyId { get; set; }
    [ValidateNever] public Party? Party { get; set; }
    public bool IsActive { get; set; } = false;
    public string ApprovalStatus { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Mobile app access rights
    public bool ShowCatalogue { get; set; } = true;
    public bool ShowPrices    { get; set; } = true;
    public bool ShowStock     { get; set; } = true;
}
