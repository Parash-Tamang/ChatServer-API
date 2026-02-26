using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using AIChatbot.Domain.Entities;
namespace AIChatbot.Infrastructure.Identity;

public static class SuperAdminSeeder
{
    public static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        var section = config.GetSection("Seed:SuperAdmin");

        var email = section["Email"];
        var password = section["Password"];
        var firstName = section["FirstName"];
        var lastName = section["LastName"];

        if (email == null || password == null) return;

        var user = await userManager.FindByEmailAsync(email);

        if (user != null) return;

        var superAdmin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName!,
            LastName = lastName!,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdmin, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"SuperAdmin creation failed: {errors}");
        }
        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
    }
}