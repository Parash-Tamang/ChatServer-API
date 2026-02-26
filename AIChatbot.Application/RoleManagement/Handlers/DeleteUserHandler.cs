using AIChatbot.Application.RoleManagement.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.RoleManagement.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.TargetUserId);
        if (user == null)
            return false;

        await _userManager.DeleteAsync(user);
        return true;
    }
}