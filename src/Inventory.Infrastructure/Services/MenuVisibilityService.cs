namespace Inventory.Infrastructure.Services;

using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class MenuVisibilityService : IMenuVisibilityService
{
    private readonly IRolePermissionRepository _repository;
    private readonly IUserRoleService _userRoleService;
    private readonly IMemoryCache _cache;

    public MenuVisibilityService(
        IRolePermissionRepository repository,
        IUserRoleService userRoleService,
        IMemoryCache cache)
    {
        _repository = repository;
        _userRoleService = userRoleService;
        _cache = cache;
    }

    public async Task<List<string>> GetConfigurableRoleNamesAsync()
    {
        var roles = await _userRoleService.GetAllRolesAsync();
        return roles
            .Where(r => r.Name != null && r.Name != "Admin")
            .Select(r => r.Name!)
            .ToList();
    }

    public async Task<MenuVisibilityVM> GetMenuVisibilityAsync(string roleName)
    {
        var saved = await _repository.GetByRoleAsync(roleName);
        var savedDict = saved.ToDictionary(p => p.Module);

        var items = AppModules.All.Select(module =>
        {
            savedDict.TryGetValue(module, out var perm);
            return new MenuItemVisibilityVM
            {
                Module        = module,
                IsMenuVisible = perm?.IsMenuVisible ?? true
            };
        }).ToList();

        return new MenuVisibilityVM { RoleName = roleName, Items = items };
    }

    public async Task SaveMenuVisibilityAsync(MenuVisibilityVM vm)
    {
        var existing = await _repository.GetByRoleAsync(vm.RoleName);
        var existingDict = existing.ToDictionary(p => p.Module);
        var visibilityDict = vm.Items.ToDictionary(i => i.Module, i => i.IsMenuVisible);

        var updated = AppModules.All.Select(module =>
        {
            existingDict.TryGetValue(module, out var perm);
            return new RolePermission
            {
                RoleName      = vm.RoleName,
                Module        = module,
                CanView       = perm?.CanView   ?? false,
                CanEdit       = perm?.CanEdit   ?? false,
                CanDelete     = perm?.CanDelete ?? false,
                IsMenuVisible = visibilityDict.TryGetValue(module, out var vis) ? vis : (perm?.IsMenuVisible ?? true)
            };
        }).ToList();

        await _repository.SaveForRoleAsync(vm.RoleName, updated);

        PermissionService.InvalidateCache(_cache, vm.RoleName);
    }
}
