using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Queries;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers;

public class GetAllDatabasesHandler : IRequestHandler<GetAllDatabasesQuery, List<ConnectionString>>
{
    private readonly IConnectionRepository _repo;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllDatabasesHandler(
        IConnectionRepository repo,
        UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    public async Task<List<ConnectionString>> Handle(GetAllDatabasesQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        var roles = await _userManager.GetRolesAsync(user!);

        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException();

        return await _repo.GetAllAsync();
    }
}