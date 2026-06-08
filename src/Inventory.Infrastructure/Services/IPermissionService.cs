namespace Inventory.Infrastructure.Services;

using System.Security.Claims;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string module, string action);
    Task<HashSet<string>> GetViewableModulesAsync(ClaimsPrincipal user);
    Task<HashSet<string>> GetMenuVisibleModulesAsync(ClaimsPrincipal user);
}
