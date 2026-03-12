using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.RoleManager;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIChatbot.web.Services
{
    public class RoleManagerService : IRoleManagerService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RoleManagerService> _logger;
        private const string BaseUrl = "/api/rolemanager/V1/role-engine";

        public RoleManagerService(ApiClient apiClient, ILogger<RoleManagerService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public async Task<List<string>> ListRoles()
        {
            try
            {
                var response = await _apiClient.GetAsync("/List all roles");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Role service returned an error");

                var data = await response.Content.ReadFromJsonAsync<RoleListDto>();

                
                if (data == null || !data.success)
                    return [];

                return data.roles ?? [];
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Unable to reach role service", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Role service request timed out", ex);
            }
        }

        public async Task<(bool Success, string Message)> CreateAdminAsync(CreateAdminRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync($"{BaseUrl}/HC2_gen", request);
                if (response.IsSuccessStatusCode)
                    return (true, "Admin created successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HC2_gen");
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, List<RoleDto> Roles, string Message)> ListRolesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync($"{BaseUrl}/LC1_listRoles");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roles = JsonSerializer.Deserialize<List<RoleDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    return (true, roles, string.Empty);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return (false, new(), "Access denied: SuperAdmin only.");
                return (false, new(), "Failed to fetch roles.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LC1_listRoles");
                return (false, new(), "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string Message)> CreateRoleAsync(CreateRoleRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync($"{BaseUrl}/LC1_gen", request);
                if (response.IsSuccessStatusCode)
                    return (true, "Role created successfully.");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return (false, "Access denied: SuperAdmin or Admin only.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LC1_gen");
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string Message)> DiscardRoleAsync(string roleId)
        {
            try
            {
                var response = await _apiClient.DeleteAsync($"{BaseUrl}/Discard_Role/{roleId}");
                if (response.IsSuccessStatusCode)
                    return (true, "Role deleted successfully.");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return (false, "Access denied: SuperAdmin only.");
                return (false, "Failed to delete role.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Discard_Role");
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, List<UserDto> Users, string Message)> ListUsersAsync(string roleId)
        {
            try
            {
                var response = await _apiClient.GetAsync($"{BaseUrl}/LC1_listUser/{roleId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    return (true, users, string.Empty);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return (false, new(), "Access denied: SuperAdmin only.");
                return (false, new(), "Failed to fetch users.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LC1_listUser");
                return (false, new(), "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string Message)> DiscardUserAsync(string userId)
        {
            try
            {
                var response = await _apiClient.DeleteAsync($"{BaseUrl}/Discard_user/{userId}");
                if (response.IsSuccessStatusCode)
                    return (true, "User deleted successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Discard_user");
                return (false, "An unexpected error occurred.");
            }
        }
    }
}