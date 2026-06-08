namespace Inventory.Infrastructure.Services;

using Inventory.Domain.ViewModels;

public interface IMenuVisibilityService
{
    Task<List<string>> GetConfigurableRoleNamesAsync();
    Task<MenuVisibilityVM> GetMenuVisibilityAsync(string roleName);
    Task SaveMenuVisibilityAsync(MenuVisibilityVM vm);
}
