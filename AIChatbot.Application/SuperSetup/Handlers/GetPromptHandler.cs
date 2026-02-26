using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Queries;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers;

public class GetPromptHandler : IRequestHandler<GetPromptQuery, PromptSet?>
{
    private readonly IPromptRepository _promptRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetPromptHandler(
        IPromptRepository promptRepo,
        UserManager<ApplicationUser> userManager)
    {
        _promptRepo = promptRepo;
        _userManager = userManager;
    }

    public async Task<PromptSet?> Handle(GetPromptQuery request, CancellationToken ct)
    {
        // 🔐 Validate requester exists
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (user == null)
            throw new Exception("User not found.");

        // 🔐 Validate role
        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException("Only Admin or SuperAdmin can access prompts.");

        // 🔍 Fetch current prompt
        var prompt = await _promptRepo.GetCurrentPromptAsync(request.DbId);

        if (prompt == null)
            throw new Exception("Current prompt set not found for this database.");

        return prompt;
    }
}