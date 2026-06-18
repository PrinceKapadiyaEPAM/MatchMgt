using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CompanyController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CompanyController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    #region Index
    public async Task<IActionResult> Index()
    {
        var profile = await _db.CompanyProfile.FirstOrDefaultAsync();

        var model = profile == null ? new CompanyProfileVM() : new CompanyProfileVM
        {
            Id = profile.Id,
            Name = profile.Name,
            LogoFileName = profile.LogoFileName,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            Pincode = profile.Pincode,
            Phone = profile.Phone,
            Mobile = profile.Mobile,
            Email = profile.Email,
            Website = profile.Website,
            GSTIN = profile.GSTIN,
            PAN = profile.PAN,
            CIN = profile.CIN,
            LetterheadHtml = profile.LetterheadHtml,
            ThemeColor = profile.ThemeColor,
            Tagline = profile.Tagline,
            BrandmarkText = profile.BrandmarkText,
        };

        return View(model);
    }
    #endregion

    #region Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CompanyProfileVM model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        // Handle logo upload
        if (model.Logo != null && model.Logo.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "company");
            Directory.CreateDirectory(uploads);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var extension = Path.GetExtension(model.Logo.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Logo), "Only jpg, jpeg, png, gif, svg files allowed.");
                return View("Index", model);
            }

            var fileName = $"logo_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(filePath);
            await model.Logo.CopyToAsync(fs);

            model.LogoFileName = fileName;
        }

        if (model.Id == Guid.Empty)
        {
            var profile = new CompanyProfile
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                LogoFileName = model.LogoFileName,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Pincode = model.Pincode,
                Phone = model.Phone,
                Mobile = model.Mobile,
                Email = model.Email,
                Website = model.Website,
                GSTIN = model.GSTIN,
                PAN = model.PAN,
                CIN = model.CIN,
                LetterheadHtml = model.LetterheadHtml,
                ThemeColor = model.ThemeColor,
                Tagline = model.Tagline,
                BrandmarkText = model.BrandmarkText,
            };
            await _db.CompanyProfile.AddAsync(profile);
        }
        else
        {
            var profile = await _db.CompanyProfile.FindAsync(model.Id);
            if (profile == null) return NotFound();

            profile.Name = model.Name;
            profile.Address = model.Address;
            profile.City = model.City;
            profile.State = model.State;
            profile.Pincode = model.Pincode;
            profile.Phone = model.Phone;
            profile.Mobile = model.Mobile;
            profile.Email = model.Email;
            profile.Website = model.Website;
            profile.GSTIN = model.GSTIN;
            profile.PAN = model.PAN;
            profile.CIN = model.CIN;
            profile.LetterheadHtml = model.LetterheadHtml;
            profile.ThemeColor = model.ThemeColor;
            profile.Tagline = model.Tagline;
            profile.BrandmarkText = model.BrandmarkText;

            if (!string.IsNullOrEmpty(model.LogoFileName))
                profile.LogoFileName = model.LogoFileName;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Company profile saved successfully.";
        return RedirectToAction(nameof(Index));
    }
    #endregion
}
