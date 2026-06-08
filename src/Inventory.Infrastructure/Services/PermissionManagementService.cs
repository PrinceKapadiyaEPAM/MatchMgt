namespace Inventory.Infrastructure.Services;

using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class PermissionManagementService : IPermissionManagementService
{
    private readonly IRolePermissionRepository _repository;
    private readonly IUserRoleService _userRoleService;
    private readonly IMemoryCache _cache;

    public PermissionManagementService(
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

    public async Task<PermissionMatrixVM> GetMatrixAsync(string roleName)
    {
        var saved = await _repository.GetByRoleAsync(roleName);
        var savedDict = saved.ToDictionary(p => p.Module);

        var modules = AppModules.All.Select(module =>
        {
            savedDict.TryGetValue(module, out var perm);
            return new ModulePermissionVM
            {
                Module    = module,
                CanView   = perm?.CanView   ?? false,
                CanEdit   = perm?.CanEdit   ?? false,
                CanDelete = perm?.CanDelete ?? false
            };
        }).ToList();

        return new PermissionMatrixVM { RoleName = roleName, Modules = modules };
    }

    public async Task SaveMatrixAsync(PermissionMatrixVM vm)
    {
        var existing = await _repository.GetByRoleAsync(vm.RoleName);
        var existingDict = existing.ToDictionary(p => p.Module);

        var permissions = vm.Modules.Select(m => new RolePermission
        {
            RoleName      = vm.RoleName,
            Module        = m.Module,
            CanView       = m.CanView,
            CanEdit       = m.CanEdit,
            CanDelete     = m.CanDelete,
            IsMenuVisible = existingDict.TryGetValue(m.Module, out var prev) ? prev.IsMenuVisible : true
        }).ToList();

        await _repository.SaveForRoleAsync(vm.RoleName, permissions);

        PermissionService.InvalidateCache(_cache, vm.RoleName);
    }
}
