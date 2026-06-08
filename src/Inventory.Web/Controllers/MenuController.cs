namespace Inventory.Web.Controllers;

using Inventory.Domain.ViewModels;
using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class MenuController : Controller
{
    private readonly IMenuVisibilityService _menuVisibilityService;

    public MenuController(IMenuVisibilityService menuVisibilityService)
    {
        _menuVisibilityService = menuVisibilityService;
    }

    public async Task<IActionResult> Index(string? role)
    {
        var roles = await _menuVisibilityService.GetConfigurableRoleNamesAsync();
        if (roles.Count == 0) return View("NoRoles");

        var selectedRole = roles.Contains(role ?? "") ? role! : roles[0];

        ViewBag.AllRoles    = roles;
        ViewBag.SelectedRole = selectedRole;

        var vm = await _menuVisibilityService.GetMenuVisibilityAsync(selectedRole);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(MenuVisibilityVM vm)
    {
        await _menuVisibilityService.SaveMenuVisibilityAsync(vm);
        TempData["Success"] = "Menu visibility saved.";
        return RedirectToAction(nameof(Index), new { role = vm.RoleName });
    }
}
