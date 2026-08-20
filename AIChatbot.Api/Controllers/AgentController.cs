using AIChatbot.Api.Models.Agent;

using AIChatbot.Application.Abstractions;

using AIChatbot.Domain.Entities;
using Microsoft.Data.SqlClient;

using System.Data;

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/agent/V1/prompt-engine")]

[AllowAnonymous]

[EnableRateLimiting("chat")]

public class AgentController : ControllerBase
{
    private readonly
        IConnectionRepository _connectionRepo;

    private readonly
        IPromptFunctionRepository _promptRepo;

    private readonly
        ILocalFunctionRepository _localRepo;

    private readonly
        UserManager<ApplicationUser>
        _userManager;

    private readonly
        RoleManager<IdentityRole>
        _roleManager;

    private readonly
        IRoleConnectionMappingRepository
        _mappingRepo;
   
    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _lookupConfigRepo;

    // ============================================================
    // FIXED NAME
    // ============================================================

    private readonly
        IRoleRuntimePermissionRepository
        _runtimeRepo;

    private readonly
        IRuntimePermissionViewRepository
        _viewRepo;

    private readonly
        IRuntimeContextResolver
        _runtimeContextResolver;

    public AgentController(
        IConnectionRepository connectionRepo,

        IPromptFunctionRepository promptRepo,

        ILocalFunctionRepository localRepo,

        UserManager<ApplicationUser> userManager,

        RoleManager<IdentityRole> roleManager,

        IRoleConnectionMappingRepository mappingRepo,

        IRoleConnectionUserLookupConfigurationRepository lookupConfigRepo,

        IRoleRuntimePermissionRepository runtimeRepo,

        IRuntimePermissionViewRepository viewRepo,

        IRuntimeContextResolver runtimeContextResolver)
    {
        _connectionRepo = connectionRepo;

        _promptRepo = promptRepo;

        _localRepo = localRepo;

        _userManager = userManager;

        _roleManager = roleManager;

        _mappingRepo = mappingRepo;

        _lookupConfigRepo = lookupConfigRepo;

        _runtimeRepo = runtimeRepo;

        _viewRepo = viewRepo;

        _runtimeContextResolver = runtimeContextResolver;
    }

    // ============================================================
    // GET ALL FUNCTIONS FOR DATABASE
    // ============================================================

    [HttpPost("Get-DB-Functions")]
    public async Task<IActionResult>
        GetDatabaseFunctions(
            [FromBody]
            GetDbPromptRequest request)
    {
        var connection =
            await _connectionRepo.GetByIdAsync(
                request.ConnectionId);

        if (connection == null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "Database not found."
            });
        }

        var globalFunctions =
            await _promptRepo
                .GetGlobalFunctionsAsync();

        var localBlocks =
            await _localRepo
                .GetByConnectionAsync(
                    connection.Id);

        var resolvedFunctions =
            globalFunctions
                .Where(g => !g.IsDeleted)
                .Select(global =>
                {
                    var local =
                        localBlocks
                            .FirstOrDefault(x =>
                                x.GlobalFunctionId
                                    == global.Id
                                &&
                                x.IsOverride);

                    return new
                    {
                        functionId =
                            global.Id,

                        functionName =
                            global.FunctionName,

                        systemPrompt =
                            local != null
                                ? local.OverridePrompt
                                : global.SystemPrompt
                    };
                })
                .ToList();

        return Ok(new
        {
            success = true,

            connectionId =
                connection.Id,

            database =
                connection.DatabaseName,

            functions =
                resolvedFunctions
        });
    }

    // ============================================================
    // GET SPECIFIC FUNCTION
    // ============================================================

    [HttpPost("Get-DB-Function-ById")]
    public async Task<IActionResult>
        GetDatabaseFunctionById(
            [FromBody]
            GetDbFunctionByIdRequest request)
    {
        var connection =
            await _connectionRepo.GetByIdAsync(
                request.ConnectionId);

        if (connection == null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "Database not found."
            });
        }

        var globalFunctions =
            await _promptRepo
                .GetGlobalFunctionsAsync();

        var global =
            globalFunctions
                .FirstOrDefault(x =>
                    x.Id == request.FunctionId);

        if (global == null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "Function not found."
            });
        }

        var localBlocks =
            await _localRepo
                .GetByConnectionAsync(
                    connection.Id);

        var local =
            localBlocks
                .FirstOrDefault(x =>
                    x.GlobalFunctionId
                        == global.Id
                    &&
                    x.IsOverride);

        return Ok(new
        {
            success = true,

            connectionId =
                connection.Id,

            database =
                connection.DatabaseName,

            function = new
            {
                functionId =
                    global.Id,

                functionName =
                    global.FunctionName,

                systemPrompt =
                    local != null
                        ? local.OverridePrompt
                        : global.SystemPrompt
            }
        });
    }

    // ============================================================
    // GET FULL RUNTIME
    // ============================================================

    [HttpPost("Get-Runtime")]
    public async Task<IActionResult>
        GetRuntime(
            [FromBody]
            GetRuntimeRequest request)
    {
        var user =
            await _userManager
                .FindByIdAsync(
                    request.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "User not found."
            });
        }

        var roleNames =
            await _userManager
                .GetRolesAsync(user);

        var roleName =
            roleNames.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "User role not found."
            });
        }

        var role =
            await _roleManager
                .FindByNameAsync(roleName);

        if (role == null)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "Role entity not found."
            });
        }

        var mappings =
            await _mappingRepo
                .GetByRoleIdAsync(
                    role.Id);

        var mapping =
            mappings.FirstOrDefault();

        if (mapping == null)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "No database assigned to role."
            });
        }
        var permissions =
    await _runtimeRepo.GetAsync(
        mapping.RoleId,
        mapping.ConnectionId);

        var runtimeView =
            await _viewRepo.GetAsync(
                mapping.RoleId,
                mapping.ConnectionId);

        var lookupConfig =
            await _lookupConfigRepo.GetAsync(
                mapping.RoleId,
                mapping.ConnectionId);

        var connection =
            await _connectionRepo.GetByIdAsync(
                mapping.ConnectionId);

        // ========================================================
        // RESOLVE RUNTIME VALUES FROM EXTERNAL USER
        // ========================================================

        var runtimeContext =
            await _runtimeContextResolver.ResolveAsync(
                user.Id,
                role.Id,
                mapping.ConnectionId);

        var runtimeValues =
            runtimeContext.Values;

        var rolePermissions =
            new Dictionary<string, object>();

        var groupedPermissions =
            permissions
                .GroupBy(x =>
                    $"{x.SchemaName}.{x.TableName}");

        foreach (var group in groupedPermissions)
        {
            var first =
                group.First();

            var filters =
                new List<object>();

            // ========================================================
            // FILTERED TABLE
            // ========================================================

            if (string.Equals(
                    first.AccessLevel,
                    "filtered",
                    StringComparison.OrdinalIgnoreCase))
            {
                foreach (var permission in group)
                {
                    object? values = null;

                    // ===============================================
                    // ID FILTER
                    // ===============================================

                    if (string.Equals(
                            permission.FilterType,
                            "id",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(
                                permission.RuntimeKey))
                        {
                            if (runtimeValues.TryGetValue(
                                permission.RuntimeKey,
                                out var runtimeValueList))
                            {
                                values = runtimeValueList;
                            }
                        }
                    }

                    // ===============================================
                    // ENUM FILTER
                    // ===============================================

                    else if (!string.IsNullOrWhiteSpace(
                                permission.FilterValuesJson))
                    {
                        values =
                            JsonSerializer.Deserialize<object>(
                                permission.FilterValuesJson);
                    }

                    filters.Add(
                        new
                        {
                            column =
                                permission.ColumnName,

                            filter_type =
                                permission.FilterType,

                            values =
                                values
                        });
                }
            }

            rolePermissions[group.Key] =
                new
                {
                    reason =
                        first.Reason,

                    access_level =
                        first.AccessLevel,

                    required_filters =
                        filters
                };
        }

        return Ok(
          new
          {
              metadata =
                  new
                  {
                      version = "1.0",

                      database =
                          connection?.DatabaseName,

                      description =
                          "Role-Based Access Control (RBAC) Permissions Dictionary",

                      note =
                          "Only columns with required filters (id, enum) are listed. Other columns are readable if the table is accessible."
                  },

              userLookupConfiguration =
                  lookupConfig != null
                      ? new
                      {
                          userTableName = lookupConfig.UserTableName,
                          userIdColumn = lookupConfig.UserIdColumn,
                          emailColumn = lookupConfig.EmailColumn,
                          phoneColumn = lookupConfig.PhoneColumn
                      }
                      : null,

              permissions =
                  new Dictionary<string, object>
                  {
                {
                    role.Name!.ToLower(),
                    rolePermissions
                }
                  }
          });
    }

    //===========================================================
    // ============================================================
    // GET MULTIPLE TABLE RUNTIME DATA
    // ============================================================

    [HttpPost("Get-Table-Runtime")]
    public async Task<IActionResult>
        GetTableRuntime(
            [FromBody]
        GetTableRuntimeRequest request)
    {
        // ========================================================
        // VALIDATE USER
        // ========================================================

        var user =
            await _userManager
                .FindByIdAsync(
                    request.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "User not found."
            });
        }

        // ========================================================
        // GET ROLE
        // ========================================================

        var roleNames =
            await _userManager
                .GetRolesAsync(user);

        var roleName =
            roleNames.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "User role not found."
            });
        }

        var role =
            await _roleManager
                .FindByNameAsync(roleName);

        if (role == null)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "Role not found."
            });
        }

        // ========================================================
        // GET DB MAPPING
        // ========================================================

        var mappings =
            await _mappingRepo
                .GetByRoleIdAsync(
                    role.Id);

        var mapping =
            mappings.FirstOrDefault();

        if (mapping == null)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "No DB assigned to role."
            });
        }

        // ========================================================
        // GET CONNECTION
        // ========================================================

        var connection =
            await _connectionRepo
                .GetByIdAsync(
                    mapping.ConnectionId);

        if (connection == null)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "Connection not found."
            });
        }

        // ========================================================
        // GET ALL RUNTIME PERMISSIONS
        // ========================================================

        var permissions =
            await _runtimeRepo.GetAsync(
                mapping.RoleId,
                mapping.ConnectionId);

        // ========================================================
        // GET RUNTIME VIEW
        // ========================================================

        var runtimeView =
            await _viewRepo.GetAsync(
                mapping.RoleId,
                mapping.ConnectionId);

        var lookupConfig =
            await _lookupConfigRepo.GetAsync(
                mapping.RoleId,
                mapping.ConnectionId);

        // ========================================================
        // RESOLVE RUNTIME VALUES FROM EXTERNAL USER
        // ========================================================

        var runtimeContext =
            await _runtimeContextResolver.ResolveAsync(
                user.Id,
                role.Id,
                mapping.ConnectionId);

        var runtimeValues =
            runtimeContext.Values;

        // ========================================================
        // BUILD TABLE PERMISSIONS RESPONSE
        // ========================================================

        var tablePermissions =
            new Dictionary<string, object>();

        var tableMessages =
            new Dictionary<string, string>();

        var groupedPermissions =
            permissions
                .GroupBy(x =>
                    $"{x.SchemaName}.{x.TableName}");

        foreach (var group in groupedPermissions)
        {
            var fullTableName = group.Key;

            // Only include requested tables
            if (!request.TableNames.Contains(fullTableName))
                continue;

            var first = group.First();

            var filters =
                new List<object>();

            // ========================================================
            // FILTERED TABLE
            // ========================================================

            if (string.Equals(
                    first.AccessLevel,
                    "filtered",
                    StringComparison.OrdinalIgnoreCase))
            {
                foreach (var permission in group)
                {
                    object? values = null;

                    // ===============================================
                    // ID FILTER
                    // ===============================================

                    if (string.Equals(
                            permission.FilterType,
                            "id",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(
                                permission.RuntimeKey))
                        {
                            if (runtimeValues.TryGetValue(
                                permission.RuntimeKey,
                                out var runtimeValueList))
                            {
                                values = runtimeValueList;
                            }
                        }
                    }

                    // ===============================================
                    // ENUM FILTER
                    // ===============================================

                    else if (!string.IsNullOrWhiteSpace(
                                permission.FilterValuesJson))
                    {
                        values =
                            JsonSerializer.Deserialize<object>(
                                permission.FilterValuesJson);
                    }

                    filters.Add(
                        new
                        {
                            column =
                                permission.ColumnName,

                            filter_type =
                                permission.FilterType,

                            values =
                                values
                        });
                }
            }

            tablePermissions[fullTableName] =
                new
                {
                    reason =
                        first.Reason,

                    access_level =
                        first.AccessLevel,

                    required_filters =
                        filters
                };
        }

        // ========================================================
        // CHECK FOR UNCONFIGURED TABLES AND BUILD MESSAGES
        // ========================================================

        foreach (var requestedTable in request.TableNames)
        {
            if (!tablePermissions.ContainsKey(requestedTable))
            {
                tableMessages[requestedTable] =
                    $"This table is not configured with lookup";
            }
        }

        // ========================================================
        // FINAL RESPONSE
        // ========================================================

        return Ok(new
        {
            success = tableMessages.Count == 0 ? "success" : "some tables not configured",

            messages =
                tableMessages.Count > 0 ? tableMessages : null,

            userId =
                user.Id,

            roleId =
                role.Id,

            connectionId =
                mapping.ConnectionId,

            userLookupConfiguration =
                lookupConfig != null
                    ? new
                    {
                        userTableName = lookupConfig.UserTableName,
                        userIdColumn = lookupConfig.UserIdColumn,
                        emailColumn = lookupConfig.EmailColumn,
                        phoneColumn = lookupConfig.PhoneColumn
                    }
                    : null,

            permissions =
                new Dictionary<string, object>
                {
                    {
                        role.Name!.ToLower(),
                        tablePermissions
                    }
                }
        });
    }
}