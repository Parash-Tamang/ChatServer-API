using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers;

/// <summary>
/// Rollback CURRENT prompts to ORIGINAL version
/// </summary>
public class RollbackPromptHandler
    : IRequestHandler<RollbackPromptCommand, bool>
{
    private readonly IPromptRepository _promptRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public RollbackPromptHandler(
        IPromptRepository promptRepo,
        UserManager<ApplicationUser> userManager)
    {
        _promptRepo = promptRepo;
        _userManager = userManager;
    }

    public async Task<bool> Handle(RollbackPromptCommand request, CancellationToken ct)
    {
        // 🔐 validate role
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (user == null)
            throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            throw new UnauthorizedAccessException("Only Admin/SuperAdmin can rollback prompts");

        // 📦 fetch prompt sets
        var original = await _promptRepo.GetOriginalPromptAsync(request.DbId);
        var current = await _promptRepo.GetCurrentPromptAsync(request.DbId);

        if (original == null || current == null)
            throw new Exception("Prompt sets not found");

        // 📦 load original functions
        var originalFunctions = await _promptRepo.GetFunctionsAsync(original.Id);

        // 🔄 rebuild CURRENT functions using ORIGINAL
        var newCurrentFunctions = originalFunctions
            .Select(fn => new PromptFunction
            {
                Id = Guid.NewGuid(),
                PromptSetId = current.Id,
                FunctionName = fn.FunctionName,
                SystemPrompt = fn.SystemPrompt
            })
            .ToList();

        await _promptRepo.ReplaceFunctionsAsync(current.Id, newCurrentFunctions);

        return true;
    }
}