namespace Inventory.Domain.Entities;

public class CompanyProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoFileName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? GSTIN { get; set; }
    public string? PAN { get; set; }
    public string? CIN { get; set; }
    public string? LetterheadHtml { get; set; }
    public string? ThemeColor { get; set; }
}
