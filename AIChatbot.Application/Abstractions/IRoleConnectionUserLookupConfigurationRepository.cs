using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRoleConnectionUserLookupConfigurationRepository
{
    Task SaveAsync(
        RoleConnectionUserLookupConfiguration entity);

    Task UpdateAsync(
        RoleConnectionUserLookupConfiguration entity);

    Task<RoleConnectionUserLookupConfiguration?>
        GetAsync(
            string roleId,
            Guid connectionId);

    Task DeleteAsync(
        string roleId,
        Guid connectionId);
}