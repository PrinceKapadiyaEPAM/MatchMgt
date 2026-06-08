namespace Inventory.Infrastructure.Services;

using Inventory.Domain.ViewModels;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRoleService _userRoleService;

    public UserManagementService(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    public async Task<List<UserListItemVM>> GetAllUsersAsync()
    {
        var users = await _userRoleService.GetAllUsersAsync();
        var result = new List<UserListItemVM>();
        foreach (var u in users)
        {
            result.Add(new UserListItemVM
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Roles = await _userRoleService.GetUserRolesAsync(u)
            });
        }
        return result;
    }

    public async Task<UserListItemVM?> GetUserByIdAsync(string id)
    {
        var user = await _userRoleService.FindUserByIdAsync(id);
        if (user == null) return null;
        return new UserListItemVM
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = await _userRoleService.GetUserRolesAsync(user)
        };
    }

    public async Task<EditUserVM?> GetUserForEditAsync(string id)
    {
        var user = await _userRoleService.FindUserByIdAsync(id);
        if (user == null) return null;
        var roles = await _userRoleService.GetUserRolesAsync(user);
        return new EditUserVM
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            SelectedRoles = roles.ToList()
        };
    }

    public async Task<List<string>> GetAllRoleNamesAsync()
    {
        var roles = await _userRoleService.GetAllRolesAsync();
        return roles.Select(r => r.Name ?? string.Empty).Where(n => n.Length > 0).ToList();
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(CreateUserVM vm)
    {
        var result = await _userRoleService.CreateUserAsync(vm.Email, vm.Password, vm.SelectedRoles);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        var user = await _userRoleService.FindUserByEmailAsync(vm.Email);
        if (user != null)
        {
            user.FullName = vm.FullName;
            await _userRoleService.UpdateUserAsync(user);
        }

        return (true, Enumerable.Empty<string>());
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(EditUserVM vm)
    {
        var user = await _userRoleService.FindUserByIdAsync(vm.Id);
        if (user == null)
            return (false, new[] { "User not found." });

        user.FullName = vm.FullName;
        user.Email = vm.Email;
        user.UserName = vm.Email;

        var updateResult = await _userRoleService.UpdateUserAsync(user);
        if (!updateResult.Succeeded)
            return (false, updateResult.Errors.Select(e => e.Description));

        var currentRoles = await _userRoleService.GetUserRolesAsync(user);
        var toAdd = vm.SelectedRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(vm.SelectedRoles).ToList();

        foreach (var role in toAdd)
            await _userRoleService.AddUserToRoleAsync(user, role);

        foreach (var role in toRemove)
            await _userRoleService.RemoveUserFromRoleAsync(user, role);

        return (true, Enumerable.Empty<string>());
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _userRoleService.DeleteUserAsync(id);
        return result.Succeeded;
    }
}
