using AIChatbot.Application.RoleManagement.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.RoleManagement.Handlers;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, bool>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateRoleHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (requester == null)
            throw new Exception("Requester not found");

        var roles = await _userManager.GetRolesAsync(requester);

        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException("Only Admin/SA can create roles");

        if (await _roleManager.RoleExistsAsync(request.RoleName))
            throw new Exception("Role already exists");

        var result = await _roleManager.CreateAsync(new IdentityRole(request.RoleName));

        if (!result.Succeeded)
            throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));

        return true;
    }
}