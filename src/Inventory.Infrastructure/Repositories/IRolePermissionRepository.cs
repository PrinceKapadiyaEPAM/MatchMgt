namespace Inventory.Infrastructure.Repositories;

using Inventory.Domain.Entities;

public interface IRolePermissionRepository
{
    Task<List<RolePermission>> GetByRoleAsync(string roleName);
    Task<List<RolePermission>> GetAllAsync();
    Task SaveForRoleAsync(string roleName, List<RolePermission> permissions);
}
