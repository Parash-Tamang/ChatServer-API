using AIChatbot.Application.Common;

namespace AIChatbot.Application.Abstractions;

public interface IRuntimeContextResolver
{
    Task<RuntimeContextResult> ResolveAsync(
        string applicationUserId,
        string roleId,
        Guid connectionId);
}