using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IExternalUserLookupService
{
    Task<ExternalUserLookupResult?> FindUserAsync(
        ConnectionString connection,
        RoleConnectionUserLookupConfiguration config,
        string email,
        string phone);
}