namespace Inventory.Infrastructure.Repositories;

using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ApplicationDbContext _db;

    public RolePermissionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<RolePermission>> GetByRoleAsync(string roleName) =>
        _db.RolePermissions.Where(p => p.RoleName == roleName).ToListAsync();

    public Task<List<RolePermission>> GetAllAsync() =>
        _db.RolePermissions.ToListAsync();

    public async Task SaveForRoleAsync(string roleName, List<RolePermission> permissions)
    {
        var existing = await _db.RolePermissions
            .Where(p => p.RoleName == roleName)
            .ToListAsync();

        _db.RolePermissions.RemoveRange(existing);
        await _db.RolePermissions.AddRangeAsync(permissions);
        await _db.SaveChangesAsync();
    }
}
