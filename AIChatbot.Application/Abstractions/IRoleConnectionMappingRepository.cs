using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRoleConnectionMappingRepository
{
    Task AddAsync(RoleConnectionMapping entity);

    Task DeleteAsync(
        string roleId,
        Guid connectionId);

    Task<bool> ExistsAsync(
        string roleId,
        Guid connectionId);

    Task<List<RoleConnectionMapping>>
        GetByRoleIdAsync(string roleId);

    Task<List<RoleConnectionMapping>>
        GetByConnectionIdAsync(Guid connectionId);
}