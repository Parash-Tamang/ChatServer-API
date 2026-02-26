using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Supersetup.Handlers;

public class DeleteConnectionHandler : IRequestHandler<DeleteConnectionCommand, bool>
{
    private readonly IConnectionRepository _repo;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteConnectionHandler(
        IConnectionRepository repo,
        UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    public async Task<bool> Handle(DeleteConnectionCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (user == null)
            throw new Exception("User not found");
        var roles = await _userManager.GetRolesAsync(user!);

        if (!roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException("Only SuperAdmin can delete DB");

        var dbConn = await _repo.GetByIdAsync(request.DbId);
        if (dbConn == null) return false;

        await _repo.DeleteAsync(dbConn);
        return true;
    }
}