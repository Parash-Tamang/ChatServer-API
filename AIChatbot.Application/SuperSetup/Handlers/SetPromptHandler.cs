using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers;

/// <summary>
/// Replace CURRENT prompt functions for a database
/// </summary>
public class SetPromptHandler : IRequestHandler<SetPromptCommand, bool>
{
    private readonly IPromptRepository _promptRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public SetPromptHandler(
        IPromptRepository promptRepo,
        UserManager<ApplicationUser> userManager)
    {
        _promptRepo = promptRepo;
        _userManager = userManager;
    }

    public async Task<bool> Handle(SetPromptCommand request, CancellationToken ct)
    {
        // 🔐 role validation
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (user == null)
            throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            throw new UnauthorizedAccessException("Only Admin/SuperAdmin can modify prompts");

        // 📦 fetch CURRENT prompt set
        var current = await _promptRepo.GetCurrentPromptAsync(request.DbId);
        if (current == null)
            throw new Exception("Current prompt set not found");

        // 🔄 replace prompt functions
        var newFunctions = request.Functions.Select(fn => new PromptFunction
        {
            Id = Guid.NewGuid(),
            PromptSetId = current.Id,
            FunctionName = fn.FunctionName,
            SystemPrompt = fn.SystemPrompt
        }).ToList();

        await _promptRepo.ReplaceFunctionsAsync(current.Id, newFunctions);

        return true;
    }
}