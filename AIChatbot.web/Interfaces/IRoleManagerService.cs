using AIChatbot.web.Dto;


namespace AIChatbot.web.Interfaces
{
    public interface IRoleManagerService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleId);
        Task<List<RoleUserDto>> GetUsersInRoleAsync(string roleId);
        Task<bool> DeleteUserAsync(string userId);
        //for assignment of the db to roles 
        Task<bool> AssignRoleToConnectionAsync(string roleId, Guid connectionId);
        Task<List<RoleConnectionDto>> GetRoleConnectionsAsync(string roleId);
    }
}