using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.ViewModels;

public class CompanyProfileVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    public string Name { get; set; } = string.Empty;

    [ValidateNever]
    public string? LogoFileName { get; set; }

    [ValidateNever]
    public IFormFile? Logo { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Website { get; set; }
    public string? GSTIN { get; set; }
    public string? PAN { get; set; }
    public string? CIN { get; set; }

    [ValidateNever]
    public string? LetterheadHtml { get; set; }

    public string? ThemeColor { get; set; }
}
