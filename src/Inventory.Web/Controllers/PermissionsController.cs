namespace Inventory.Web.Controllers;

using Inventory.Domain.ViewModels;
using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class PermissionsController : Controller
{
    private readonly IPermissionManagementService _permissionManagementService;

    public PermissionsController(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<IActionResult> Index(string? role)
    {
        var roles = await _permissionManagementService.GetConfigurableRoleNamesAsync();
        if (roles.Count == 0)
            return View("NoRoles");

        var selectedRole = roles.Contains(role ?? "") ? role! : roles[0];
        var matrix = await _permissionManagementService.GetMatrixAsync(selectedRole);

        ViewBag.AllRoles = roles;
        ViewBag.SelectedRole = selectedRole;

        return View(matrix);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PermissionMatrixVM vm)
    {
        await _permissionManagementService.SaveMatrixAsync(vm);
        return RedirectToAction(nameof(Index), new { role = vm.RoleName });
    }
}
