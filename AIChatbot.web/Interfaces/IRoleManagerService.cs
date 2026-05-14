using AIChatbot.Web.Dto;

namespace AIChatbot.Web.Interfaces
{
    public interface IRoleManagerService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleId);
        Task<List<RoleUserDto>> GetUsersInRoleAsync(string roleId);
        Task<bool> DeleteUserAsync(string userId);
    }
}