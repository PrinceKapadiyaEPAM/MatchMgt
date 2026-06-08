namespace Inventory.Infrastructure.Services;

using Inventory.Domain.ViewModels;

public interface IPermissionManagementService
{
    Task<List<string>> GetConfigurableRoleNamesAsync();
    Task<PermissionMatrixVM> GetMatrixAsync(string roleName);
    Task SaveMatrixAsync(PermissionMatrixVM vm);
}
