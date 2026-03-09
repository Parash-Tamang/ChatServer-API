using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IAiProviderService
{
    Task<DatabaseSetupResult> PrepareDatabaseAsync(ConnectionString connection);

    Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema);
}