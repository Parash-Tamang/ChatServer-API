using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRoleRuntimePermissionRepository
{
    Task SaveAsync(
        List<RoleRuntimePermission> entities);

    Task<List<RoleRuntimePermission>> GetAsync(
        string roleId,
        Guid connectionId);

    Task DeleteAsync(
        string roleId,
        Guid connectionId);
}