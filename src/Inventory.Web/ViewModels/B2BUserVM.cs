using Inventory.Domain.Entities;

namespace Inventory.Web.ViewModels;

public class B2BUserEditVM
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public int? PartyId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Party> Parties { get; set; } = new();
}
