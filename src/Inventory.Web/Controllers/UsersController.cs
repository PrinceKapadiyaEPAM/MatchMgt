namespace Inventory.Web.Controllers;

using Inventory.Domain.ViewModels;
using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManagementService.GetAllUsersAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
        return View(new CreateUserVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
            return View(vm);
        }

        var (success, errors) = await _userManagementService.CreateUserAsync(vm);
        if (!success)
        {
            foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
            ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var vm = await _userManagementService.GetUserForEditAsync(id);
        if (vm == null) return NotFound();

        ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
            return View(vm);
        }

        var (success, errors) = await _userManagementService.UpdateUserAsync(vm);
        if (!success)
        {
            foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
            ViewBag.AllRoles = await _userManagementService.GetAllRoleNamesAsync();
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _userManagementService.DeleteUserAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
