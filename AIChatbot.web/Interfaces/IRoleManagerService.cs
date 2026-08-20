using AIChatbot.web.Dto;
using System.Text.Json.Serialization;


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
        Task<List<RoleLookupDto>> GetRolesByConnectionAsync(Guid connectionId);
        Task<List<SchemaTableDto>> GetSchemaAsync(Guid connectionId);
        Task<RuntimePermissionsPayloadDto?> GetRuntimePermissionsAsync(string roleId, Guid connectionId);
        Task<bool> SaveRuntimePermissionsAsync(RuntimePermissionsPayloadDto payload);
        Task<UserLookupConfigurationDto?> GetUserLookupConfigurationAsync(string roleId, Guid connectionId);
        Task<bool> SaveUserLookupConfigurationAsync(SaveUserLookupConfigurationDto request);
        Task<bool> DeleteUserLookupConfigurationAsync(string roleId, Guid connectionId);
    }

    public class RoleLookupDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class SchemaTableDto
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
    }

    public class RuntimePermissionDto
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string FilterType { get; set; } = string.Empty;
        public string? RuntimeKey { get; set; }
        public List<string>? Values { get; set; }
    }

    public class RuntimePermissionsPayloadDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        
        [JsonPropertyName("selectedRoleId")]
        public string SelectedRoleId { get; set; } = string.Empty;
        
        [JsonPropertyName("database")]
        public string Database { get; set; } = string.Empty;
        
        [JsonPropertyName("connectionId")]
        public Guid ConnectionId { get; set; }
        
        [JsonPropertyName("permissions")]
        public Dictionary<string, RuntimePermissionEntryDto> Permissions { get; set; } = new();
    }

    public class RuntimePermissionEntryDto
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
        
        [JsonPropertyName("access_level")]
        public string AccessLevel { get; set; } = "unrestricted";
        
        [JsonPropertyName("required_filters")]
        public List<RuntimePermissionFilterDto> RequiredFilters { get; set; } = new();
    }

    public class RuntimePermissionFilterDto
    {
        [JsonPropertyName("column")]
        public string Column { get; set; } = string.Empty;
        
        [JsonPropertyName("filter_type")]
        public string FilterType { get; set; } = "id";
        
        [JsonPropertyName("values")]
        public object? Values { get; set; }
    }
}