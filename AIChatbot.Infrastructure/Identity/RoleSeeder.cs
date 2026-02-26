using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "SuperAdmin",
            "Admin",
            "GeneralUser"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        }
    }
}