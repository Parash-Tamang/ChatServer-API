using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AppDbContext _db;

    public RolePermissionRepository(AppDbContext db)
    {
        _db = db;
    }

    // ===============================
    // DB LEVEL ACCESS
    // ===============================
    public async Task<List<Guid>> GetDbPermissionsAsync(string roleName)
    {
        return await _db.RoleDbPermissions
            .Where(x => x.RoleName == roleName)
            .Select(x => x.ConnectionStringId)
            .ToListAsync();
    }

    // ===============================
    // TABLE LEVEL ACCESS
    // ===============================
    public async Task<List<string>> GetTablePermissionsAsync(string roleName, Guid connectionId)
    {
        return await _db.RoleTablePermissions
            .Where(x => x.RoleName == roleName &&
                        x.ConnectionStringId == connectionId)
            .Select(x => x.TableName)
            .ToListAsync();
    }

    // ===============================
    // COLUMN LEVEL ACCESS
    // ===============================
    public async Task<List<string>> GetColumnPermissionsAsync(
        string roleName,
        Guid connectionId,
        string tableName)
    {
        return await _db.RoleColumnPermissions
            .Where(x => x.RoleName == roleName &&
                        x.ConnectionStringId == connectionId &&
                        x.TableName == tableName)
            .Select(x => x.ColumnName)
            .ToListAsync();
    }

    // ===============================
    // ASSIGN DB PERMISSION
    // ===============================
    public async Task AddDbPermissionAsync(RoleDbPermission permission)
    {
        _db.RoleDbPermissions.Add(permission);
        await _db.SaveChangesAsync();
    }

    // ===============================
    // ASSIGN TABLE PERMISSION
    // ===============================
    public async Task AddTablePermissionAsync(RoleTablePermission permission)
    {
        _db.RoleTablePermissions.Add(permission);
        await _db.SaveChangesAsync();
    }

    // ===============================
    // ASSIGN COLUMN PERMISSION
    // ===============================
    public async Task AddColumnPermissionAsync(RoleColumnPermission permission)
    {
        _db.RoleColumnPermissions.Add(permission);
        await _db.SaveChangesAsync();
    }
}