using AIChatbot.Application.RoleManagement.Commands;
using Microsoft.AspNetCore.Identity;
using MediatR;
using AIChatbot.Domain.Entities;


namespace AIChatbot.Application.RoleManagement.Handlers;

public class CreateAdminHandler : IRequestHandler<CreateAdminCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateAdminHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(CreateAdminCommand request, CancellationToken ct)
    {
        // 🔐 requester validation
        var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (requester == null)
            throw new Exception("Requester not found");

        var requesterRoles = await _userManager.GetRolesAsync(requester);

        if (!requesterRoles.Contains("SuperAdmin") && !requesterRoles.Contains("Admin"))
            throw new UnauthorizedAccessException("Only Admin/SuperAdmin can create Admin users");

        // 🔍 check duplicate email
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new Exception("User already exists with this email");

        // 🧱 create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.Phone,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));

        // 🎯 assign Admin role by default
        if (!await _roleManager.RoleExistsAsync("Admin"))
            throw new Exception("Admin role not seeded");

        await _userManager.AddToRoleAsync(user, "Admin");

        return true;
    }
}