namespace Inventory.Web.Filters;

using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string module, string action)
        : base(typeof(PermissionFilter))
    {
        Arguments = [module, action];
    }
}

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;
    private readonly string _module;
    private readonly string _action;

    public PermissionFilter(IPermissionService permissionService, string module, string action)
    {
        _permissionService = permissionService;
        _module = module;
        _action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
            return;

        var allowed = await _permissionService.HasPermissionAsync(user, _module, _action);
        if (!allowed)
            context.Result = new RedirectResult("/Identity/Account/AccessDenied");
    }
}
