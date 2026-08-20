using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IAiProviderService
{
    // ============================================================
    // KNOWLEDGEBASE
    // ============================================================

    Task<DatabaseSetupResult> PrepareDatabaseAsync(
        ConnectionString connection);

    Task<DatabaseSetupResult> UpdateDatabaseAsync(
        ConnectionString connection);

    Task<bool> NotifyConnectionDeletedAsync(
        Guid connectionId);

    // ============================================================
    // CHAT
    // ============================================================

    Task<LlmResponse> GetReplyAsync(
        string userId,
        string userRole,
        string userQuery,
        Guid connectionId,
        IEnumerable<Message> history,
        object? sessionContext = null);
}