using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRuntimePermissionViewRepository
{
    Task AddAsync(
        RuntimePermissionView entity);

    Task<RuntimePermissionView?> GetAsync(
        string roleId,
        Guid connectionId);

    Task DeleteAsync(
        RuntimePermissionView entity);
}