namespace Inventory.Web.Controllers;

using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly IUserRoleService _userRoleService;

    public RolesController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _userRoleService.GetAllRolesAsync();
        return View(roles);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName))
            await _userRoleService.CreateRoleAsync(roleName);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string roleName)
    {
        await _userRoleService.DeleteRoleAsync(roleName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AssignRole(string userId)
    {
        var userVm = await _userRoleService.FindUserByIdAsync(userId);
        if (userVm == null) return NotFound();

        ViewBag.Roles = await _userRoleService.GetAllRolesAsync();
        return View(userVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userRoleService.FindUserByIdAsync(userId);
        if (user != null)
            await _userRoleService.AddUserToRoleAsync(user, role);

        return RedirectToAction("Index", "Users");
    }
}
