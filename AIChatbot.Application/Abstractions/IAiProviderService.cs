using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;


/// Abstraction over external AI provider (e.g. OpenAI, Azure OpenAI, etc.)
/// Responsible only for generating assistant replies


public interface IAiProviderService
{
    Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult Schema);
}

