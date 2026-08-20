using AIChatbot.Application.RoleManagement.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.RoleManagement.Handlers;

public class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteRoleHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> Handle(
        DeleteRoleCommand request,
        CancellationToken ct)
    {
        var requester =
            await _userManager.FindByIdAsync(
                request.RequestedByUserId);

        if (requester == null)
            throw new Exception("Requester not found");

        var roles =
            await _userManager.GetRolesAsync(
                requester);

        if (!roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException(
                "Only SuperAdmin can delete roles");

        var role =
            await _roleManager.FindByIdAsync(
                request.RoleId);

        if (role == null)
        {
            throw new KeyNotFoundException(
                "Role not found.");
        }

        var protectedRoles =
            new[]
            {
                "SuperAdmin",
                "SA"
            };

        if (protectedRoles.Contains(
                role.Name,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Protected role cannot be deleted.");
        }

        var result =
            await _roleManager.DeleteAsync(
                role);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(
                    ",",
                    result.Errors.Select(
                        e => e.Description)));
        }

        return true;
    }
}