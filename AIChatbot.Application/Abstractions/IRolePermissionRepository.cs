using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRolePermissionRepository
{
    // DB level access
    Task<List<Guid>> GetDbPermissionsAsync(string roleName);

    // Table level access
    Task<List<string>> GetTablePermissionsAsync(string roleName, Guid connectionId);

    // Column level access
    Task<List<string>> GetColumnPermissionsAsync(string roleName, Guid connectionId, string tableName);

    // Assign permissions
    Task AddDbPermissionAsync(RoleDbPermission permission);
    Task AddTablePermissionAsync(RoleTablePermission permission);
    Task AddColumnPermissionAsync(RoleColumnPermission permission);
}