using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleAccess.Models;

namespace AIChatbot.Application.RoleAccess.Services;

public class RoleAccessService : IRoleAccessService
{
    private readonly IRolePermissionRepository _permissionRepo;

    public RoleAccessService(IRolePermissionRepository permissionRepo)
    {
        _permissionRepo = permissionRepo;
    }

    public async Task<RoleAccessResult> GetAccessAsync(string roleName)
    {
        var result = new RoleAccessResult();

        // 1️⃣ DB ACCESS
        var dbs = await _permissionRepo.GetDbPermissionsAsync(roleName);
        result.Databases = dbs;

        foreach (var dbId in dbs)
        {
            // 2️⃣ TABLE ACCESS
            var tables = await _permissionRepo.GetTablePermissionsAsync(roleName, dbId);
            result.Tables[dbId] = tables;

            result.Columns[dbId] = new Dictionary<string, List<string>>();

            foreach (var table in tables)
            {
                // 3️⃣ COLUMN ACCESS
                var columns = await _permissionRepo.GetColumnPermissionsAsync(roleName, dbId, table);

                // 🔥 IMPORTANT RULE
                // if no column defined → full table allowed
                result.Columns[dbId][table] = columns.Count == 0
                    ? new List<string>() // means full table
                    : columns;
            }
        }

        return result;
    }
}