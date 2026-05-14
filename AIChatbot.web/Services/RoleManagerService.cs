using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using AIChatbot.Web.Dto;
using AIChatbot.Web.Interfaces;
using System.Text.Json;

namespace AIChatbot.web.Services
{
    public class RoleManagerService : IRoleManagerService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RoleManagerService> _logger;

        public RoleManagerService(ApiClient apiClient, ILogger<RoleManagerService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync("/api/rolemanager/V1/role-engine/LC1_listRoles");
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roles = JsonSerializer.Deserialize<List<RoleDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return roles ?? new List<RoleDto>();
                }

                _logger.LogWarning("GetAllRoles failed. Status: {Status}", response?.StatusCode);
                return new List<RoleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles");
                return new List<RoleDto>();
            }
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            try
            {
                var response = await _apiClient.PostAsync(
                    "/api/rolemanager/V1/role-engine/LC1_gen",
                    new { roleName }
                );
                return response is { IsSuccessStatusCode: true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", roleName);
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            try
            {
                var response = await _apiClient.DeleteWithBodyAsync(
                    $"/api/rolemanager/V1/role-engine/Discard_Role/{roleId}",
                    new { roleId }
                );
                return response is { IsSuccessStatusCode: true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                return false;
            }
        }

        public async Task<List<RoleUserDto>> GetUsersInRoleAsync(string roleId)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"/api/rolemanager/V1/role-engine/LC1_listUser/{roleId}"
                );
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("GetUsersInRole response: {Json}", json);
                    var users = JsonSerializer.Deserialize<List<RoleUserDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return users ?? new List<RoleUserDto>();
                }
                _logger.LogWarning("GetUsersInRole failed. Status: {Status}", response?.StatusCode);
                return new List<RoleUserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users for role: {RoleId}", roleId);
                return new List<RoleUserDto>();
            }
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _apiClient.DeleteWithBodyAsync(
                    $"/api/rolemanager/V1/role-engine/Discard_user/{userId}",
                    new { userId }
                );
                return response is { IsSuccessStatusCode: true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }
    }
}