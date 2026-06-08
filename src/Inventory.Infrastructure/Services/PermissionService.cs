namespace Inventory.Infrastructure.Services;

using System.Security.Claims;
using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class PermissionService : IPermissionService
{
    private readonly IRolePermissionRepository _repository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PermissionService(IRolePermissionRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string module, string action)
    {
        if (user.IsInRole("Admin")) return true;

        var roles = GetUserRoles(user);
        foreach (var role in roles)
        {
            var perms = await GetCachedPermissionsAsync(role);
            var perm = perms.FirstOrDefault(p => p.Module == module);
            if (perm == null) continue;

            var granted = action switch
            {
                "View"   => perm.CanView,
                "Edit"   => perm.CanEdit,
                "Delete" => perm.CanDelete,
                _        => false
            };

            if (granted) return true;
        }

        return false;
    }

    public async Task<HashSet<string>> GetViewableModulesAsync(ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin"))
            return new HashSet<string>(AppModules.All);

        var result = new HashSet<string>();
        var roles = GetUserRoles(user);

        foreach (var role in roles)
        {
            var perms = await GetCachedPermissionsAsync(role);
            foreach (var p in perms.Where(p => p.CanView))
                result.Add(p.Module);
        }

        return result;
    }

    public async Task<HashSet<string>> GetMenuVisibleModulesAsync(ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin"))
            return new HashSet<string>(AppModules.All);

        var result = new HashSet<string>();
        var roles = GetUserRoles(user);

        foreach (var role in roles)
        {
            var perms = await GetCachedPermissionsAsync(role);
            foreach (var p in perms.Where(p => p.IsMenuVisible))
                result.Add(p.Module);
        }

        return result;
    }

    public static void InvalidateCache(IMemoryCache cache, string roleName) =>
        cache.Remove(CacheKey(roleName));

    private async Task<List<RolePermission>> GetCachedPermissionsAsync(string roleName)
    {
        return await _cache.GetOrCreateAsync(CacheKey(roleName), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _repository.GetByRoleAsync(roleName);
        }) ?? new List<RolePermission>();
    }

    private static IEnumerable<string> GetUserRoles(ClaimsPrincipal user) =>
        user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

    private static string CacheKey(string roleName) => $"perms:{roleName}";
}
