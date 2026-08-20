using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;

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

        public async Task<UserLookupConfigurationDto?> GetUserLookupConfigurationAsync(string roleId, Guid connectionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId) || connectionId == Guid.Empty)
                    return null;

                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/lookup-for-user-using/{roleId}/{connectionId}"
                );

               if (response is { IsSuccessStatusCode: true })
{
    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        return null; // 204 = no config exists yet, not an error

    var json = await response.Content.ReadAsStringAsync();
    if (string.IsNullOrWhiteSpace(json))
        return null;

    var lookup = JsonSerializer.Deserialize<UserLookupConfigurationDto>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return lookup;
}
                _logger.LogWarning("GetUserLookupConfiguration failed. Status: {Status}", response?.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching lookup configuration for role {RoleId} connection {ConnectionId}", roleId, connectionId);
                return null;
            }
        }

        public async Task<bool> SaveUserLookupConfigurationAsync(SaveUserLookupConfigurationDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RoleId) || request.ConnectionId == Guid.Empty || string.IsNullOrWhiteSpace(request.UserTableName) || string.IsNullOrWhiteSpace(request.UserIdColumn))
                    return false;

                var response = await _apiClient.PostAsync(
                    "/api/access/V1/Data-Setup-engine/lookup-for-user-using/save",
                    new
                    {
                        roleId = request.RoleId,
                        connectionId = request.ConnectionId,
                        userTableName = request.UserTableName,
                        userIdColumn = request.UserIdColumn,
                        emailColumn = request.EmailColumn,
                        phoneColumn = request.PhoneColumn,
                    }
                );

                if (response is { IsSuccessStatusCode: true })
                    return true;

                _logger.LogWarning("SaveUserLookupConfiguration failed. Status: {Status}", response?.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving lookup configuration for role {RoleId} connection {ConnectionId}", request.RoleId, request.ConnectionId);
                return false;
            }
        }

        public async Task<bool> DeleteUserLookupConfigurationAsync(string roleId, Guid connectionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId) || connectionId == Guid.Empty)
                    return false;

                var response = await _apiClient.DeleteAsync(
                    $"/api/access/V1/Data-Setup-engine/lookup-for-user-using/{roleId}/{connectionId}"
                );

                if (response is { IsSuccessStatusCode: true })
                    return true;

                _logger.LogWarning("DeleteUserLookupConfiguration failed. Status: {Status}", response?.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lookup configuration for role {RoleId} connection {ConnectionId}", roleId, connectionId);
                return false;
            }
        }

        //for givingte acess to te roles
        public async Task<bool> AssignRoleToConnectionAsync(string roleId, Guid connectionId)
        {
            try
            {
                var response = await _apiClient.PostAsync(
                    "/api/access/V1/Data-Setup-engine/Role-and-DB/assign",
                    new { roleId, connectionId }
                );
                return response is { IsSuccessStatusCode: true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to connection {ConnectionId}", roleId, connectionId);
                return false;
            }
        }


        // ✅ NEW
        public async Task<List<RoleConnectionDto>> GetRoleConnectionsAsync(string roleId)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/Role-and-DB/{roleId}"
                );
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("GetRoleConnections response: {Json}", json);
                    var result = JsonSerializer.Deserialize<List<RoleConnectionDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result ?? new List<RoleConnectionDto>();
                }
                _logger.LogWarning("GetRoleConnections failed. Status: {Status}", response?.StatusCode);
                return new List<RoleConnectionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching connections for role: {RoleId}", roleId);
                return new List<RoleConnectionDto>();
            }
        }

        public async Task<List<Interfaces.RoleLookupDto>> GetRolesByConnectionAsync(Guid connectionId)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"/api/rolemanager/V1/role-engine/roles-by-connection/{connectionId}"
                );
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("GetRolesByConnection response: {Json}", json);
                    var roles = JsonSerializer.Deserialize<List<Interfaces.RoleLookupDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return roles ?? new List<Interfaces.RoleLookupDto>();
                }
                _logger.LogWarning("GetRolesByConnection failed. Status: {Status}", response?.StatusCode);
                return new List<Interfaces.RoleLookupDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles for connection: {ConnectionId}", connectionId);
                return new List<Interfaces.RoleLookupDto>();
            }
        }

        public async Task<List<Interfaces.SchemaTableDto>> GetSchemaAsync(Guid connectionId)
        {
            try
            {
                if (connectionId == Guid.Empty)
                    return new List<Interfaces.SchemaTableDto>();
                    
                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/GETschema/{connectionId}"
                );
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("GetSchema response received");
                    var schema = JsonSerializer.Deserialize<List<Interfaces.SchemaTableDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return schema ?? new List<Interfaces.SchemaTableDto>();
                }
                _logger.LogWarning("GetSchema failed. Status: {Status}", response?.StatusCode);
                return new List<Interfaces.SchemaTableDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schema");
                return new List<Interfaces.SchemaTableDto>();
            }
        }

        public async Task<Interfaces.RuntimePermissionsPayloadDto?> GetRuntimePermissionsAsync(string roleId, Guid connectionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId) || connectionId == Guid.Empty)
                    return null;

                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/runtime-access/{roleId}/{connectionId}"
                );
                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("GetRuntimePermissions response received");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var payload = DeserializeRuntimePermissionsPayload(json, options, roleId, connectionId);
                    if (payload != null)
                        return payload;

                    _logger.LogWarning("GetRuntimePermissions returned an unexpected shape for role {RoleId} and connection {ConnectionId}", roleId, connectionId);
                    return null;
                }

                _logger.LogWarning("GetRuntimePermissions failed. Status: {Status}", response?.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching runtime permissions for role {RoleId} and connection {ConnectionId}", roleId, connectionId);
                return null;
            }
        }

        public async Task<bool> SaveRuntimePermissionsAsync(Interfaces.RuntimePermissionsPayloadDto payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.SelectedRoleId) || payload.ConnectionId == Guid.Empty)
                    return false;

                // var request = new
                // {
                //     roleId = payload.SelectedRoleId,
                //     connectionId = payload.ConnectionId,
                //     permissions = payload.Permissions
                //         .SelectMany(pair => ConvertPermissionEntry(pair.Key, pair.Value))
                //         .ToList()
                // };
             
                var response = await _apiClient.PostAsync(
                    "/api/access/V1/Data-Setup-engine/runtime-access/save",
                    payload
                );

                if (response is { IsSuccessStatusCode: true })
                    return true;

                _logger.LogWarning("SaveRuntimePermissions failed. Status: {Status}", response?.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving runtime permissions for role {RoleId} and connection {ConnectionId}", payload.SelectedRoleId, payload.ConnectionId);
                return false;
            }
        }

        private static IEnumerable<object> ConvertPermissionEntry(string key, Interfaces.RuntimePermissionEntryDto entry)
        {
            var separatorIndex = key.IndexOf('.');
            if (separatorIndex <= 0)
                yield break;

            var schemaName = key.Substring(0, separatorIndex);
            var tableName = key.Substring(separatorIndex + 1);

            if (entry.AccessLevel == null || entry.AccessLevel == "unrestricted")
                yield break;

            if (entry.RequiredFilters == null || !entry.RequiredFilters.Any())
                yield break;

            foreach (var filter in entry.RequiredFilters)
            {
                var values = NormalizeSaveValues(filter.Values);
                yield return new
                {
                    schemaName,
                    tableName,
                    columnName = filter.Column,
                    filterType = filter.FilterType ?? "id",
                    runtimeKey = (string?)null,
                    values
                };
            }
        }

        private static List<string>? NormalizeSaveValues(object? values)
        {
            if (values == null)
                return null;

            if (values is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => new List<string> { jsonElement.GetString() ?? string.Empty },
                    JsonValueKind.Array => jsonElement.EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.String)
                        .Select(item => item.GetString() ?? string.Empty)
                        .ToList(),
                    _ => new List<string> { jsonElement.ToString() }
                };
            }

            if (values is string str)
            {
                return str.Split(',')
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
            }

            if (values is IEnumerable<string> stringValues)
            {
                return stringValues.ToList();
            }

            if (values is IEnumerable<object> objectValues)
            {
                return objectValues.Select(v => v?.ToString() ?? string.Empty).ToList();
            }

            return new List<string> { values.ToString() ?? string.Empty };
        }

        private Interfaces.RuntimePermissionsPayloadDto? DeserializeRuntimePermissionsPayload(string json, JsonSerializerOptions options, string roleId, Guid connectionId)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    var rows = JsonSerializer.Deserialize<List<RawRuntimePermissionDto>>(root.GetRawText(), options);
                    return rows == null ? null : ConvertRowsToPayload(rows, roleId, connectionId);
                }

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("permissions", out var permissionsElement))
                    {
                        var payload = JsonSerializer.Deserialize<Interfaces.RuntimePermissionsPayloadDto>(root.GetRawText(), options);
                        if (payload != null)
                            return payload;

                        // Fallback: create the expected payload object manually
                        var manualPayload = new Interfaces.RuntimePermissionsPayloadDto
                        {
                            Role = root.TryGetProperty("role", out var roleElement) ? roleElement.GetString() ?? string.Empty : string.Empty,
                            SelectedRoleId = root.TryGetProperty("selectedRoleId", out var selectedRoleIdElement) ? selectedRoleIdElement.GetString() ?? string.Empty : roleId,
                            Database = root.TryGetProperty("database", out var databaseElement) ? databaseElement.GetString() ?? string.Empty : string.Empty,
                            ConnectionId = root.TryGetProperty("connectionId", out var connectionIdElement) && connectionIdElement.ValueKind == JsonValueKind.String && Guid.TryParse(connectionIdElement.GetString(), out var parsedConnectionId)
                                ? parsedConnectionId
                                : connectionId,
                            Permissions = ParsePermissionsMap(permissionsElement, options)
                        };

                        return manualPayload;
                    }

                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.ValueKind == JsonValueKind.Array)
                        {
                            var rows = JsonSerializer.Deserialize<List<RawRuntimePermissionDto>>(dataElement.GetRawText(), options);
                            return rows == null ? null : ConvertRowsToPayload(rows, roleId, connectionId);
                        }

                        if (dataElement.ValueKind == JsonValueKind.Object)
                        {
                            if (dataElement.TryGetProperty("permissions", out var dataPermissionsElement))
                            {
                                var payload = JsonSerializer.Deserialize<Interfaces.RuntimePermissionsPayloadDto>(dataElement.GetRawText(), options);
                                if (payload != null)
                                    return payload;

                                return new Interfaces.RuntimePermissionsPayloadDto
                                {
                                    Role = dataElement.TryGetProperty("role", out var roleElement) ? roleElement.GetString() ?? string.Empty : string.Empty,
                                    SelectedRoleId = dataElement.TryGetProperty("selectedRoleId", out var selectedRoleIdElement) ? selectedRoleIdElement.GetString() ?? string.Empty : roleId,
                                    Database = dataElement.TryGetProperty("database", out var databaseElement) ? databaseElement.GetString() ?? string.Empty : string.Empty,
                                    ConnectionId = dataElement.TryGetProperty("connectionId", out var connectionIdElement) && connectionIdElement.ValueKind == JsonValueKind.String && Guid.TryParse(connectionIdElement.GetString(), out var parsedConnectionId)
                                        ? parsedConnectionId
                                        : connectionId,
                                    Permissions = ParsePermissionsMap(dataPermissionsElement, options)
                                };
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse runtime permissions payload JSON.");
            }

            return null;
        }

        private static Interfaces.RuntimePermissionsPayloadDto ConvertRowsToPayload(List<RawRuntimePermissionDto> rows, string roleId, Guid connectionId)
        {
            var payload = new Interfaces.RuntimePermissionsPayloadDto
            {
                SelectedRoleId = roleId,
                ConnectionId = connectionId,
                Permissions = new Dictionary<string, Interfaces.RuntimePermissionEntryDto>()
            };

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.SchemaName) || string.IsNullOrWhiteSpace(row.TableName))
                    continue;

                var key = $"{row.SchemaName}.{row.TableName}";
                if (!payload.Permissions.TryGetValue(key, out var entry))
                {
                    entry = new Interfaces.RuntimePermissionEntryDto
                    {
                        Reason = string.Empty,
                        AccessLevel = "unrestricted",
                        RequiredFilters = new List<Interfaces.RuntimePermissionFilterDto>()
                    };
                    payload.Permissions[key] = entry;
                }

                if (!string.IsNullOrWhiteSpace(row.FilterType) && row.FilterType != "unrestricted")
                {
                    entry.AccessLevel = "filtered";
                    entry.RequiredFilters.Add(new Interfaces.RuntimePermissionFilterDto
                    {
                        Column = row.ColumnName ?? string.Empty,
                        FilterType = row.FilterType,
                        Values = row.Values.ValueKind == JsonValueKind.Undefined || row.Values.ValueKind == JsonValueKind.Null
                            ? string.Empty
                            : (object)row.Values
                    });
                }
            }

            return payload;
        }

        private static Dictionary<string, Interfaces.RuntimePermissionEntryDto> ParsePermissionsMap(JsonElement permissionsElement, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, Interfaces.RuntimePermissionEntryDto>();
            foreach (var property in permissionsElement.EnumerateObject())
            {
                var entry = JsonSerializer.Deserialize<Interfaces.RuntimePermissionEntryDto>(property.Value.GetRawText(), options);
                if (entry != null)
                {
                    result[property.Name] = entry;
                }
            }
            return result;
        }

        private static List<string>? NormalizeValues(JsonElement valuesElement)
        {
            if (valuesElement.ValueKind == JsonValueKind.Undefined || valuesElement.ValueKind == JsonValueKind.Null)
                return null;

            if (valuesElement.ValueKind == JsonValueKind.Array)
            {
                return valuesElement.EnumerateArray()
                    .Select(v => v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : v.GetRawText())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
            }

            if (valuesElement.ValueKind == JsonValueKind.String)
                return new List<string> { valuesElement.GetString() ?? string.Empty };

            return new List<string> { valuesElement.GetRawText() };
        }

        private class RawRuntimePermissionDto
        {
            public string? SchemaName { get; set; }
            public string? TableName { get; set; }
            public string? ColumnName { get; set; }
            public string? FilterType { get; set; }
            public string? RuntimeKey { get; set; }
            public JsonElement Values { get; set; }
        }

    }
}

