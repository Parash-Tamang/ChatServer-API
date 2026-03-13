using AIChatbot.web.Dto;
using AIChatbot.web.Models.RoleManager;

namespace AIChatbot.web.Interfaces
{
    public interface  IRoleManagerService
    {
        Task<(bool Success, string Message)> CreateAdminAsync(CreateAdminRequest request);
        Task<(bool Success, List<RoleDto> Roles, string Message)> ListRolesAsync();
        Task<(bool Success, string Message)> CreateRoleAsync(CreateRoleRequest request);
        Task<(bool Success, string Message)> DiscardRoleAsync(string roleId);
        Task<(bool Success, List<UserDto> Users, string Message)> ListUsersAsync(string roleId);
        Task<(bool Success, string Message)> DiscardUserAsync(string userId);

        Task<RoleListDto> ListRoles();
    }
}
