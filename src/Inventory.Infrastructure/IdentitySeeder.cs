namespace Inventory.Infrastructure;

using Inventory.Domain.Constants;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "StoreManager" };

        // Create roles
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Default admin
        var adminEmail = "admin@ambit.com";
        var adminPassword = "Admin@123";

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, adminPassword);
            await userManager.AddToRoleAsync(user, "Admin");
        }

        // Seed default StoreManager permissions (View + Edit, no Delete)
        var db = services.GetRequiredService<ApplicationDbContext>();
        var hasPerms = db.RolePermissions.Any(p => p.RoleName == "StoreManager");
        if (!hasPerms)
        {
            var defaults = AppModules.All.Select(module => new RolePermission
            {
                RoleName      = "StoreManager",
                Module        = module,
                CanView       = true,
                CanEdit       = true,
                CanDelete     = false,
                IsMenuVisible = true
            });
            db.RolePermissions.AddRange(defaults);
            await db.SaveChangesAsync();
        }
    }
}