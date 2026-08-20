using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RoleRuntimePermissionRepository
    : IRoleRuntimePermissionRepository
{
    private readonly AppDbContext _context;

    public RoleRuntimePermissionRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        List<RoleRuntimePermission> entities)
    {
        await _context
            .RoleRuntimePermissions
            .AddRangeAsync(entities);

        await _context.SaveChangesAsync();
    }

    public async Task<List<RoleRuntimePermission>>
        GetAsync(
            string roleId,
            Guid connectionId)
    {
        return await _context
            .RoleRuntimePermissions
            .Where(x =>
                x.RoleId == roleId &&
                x.ConnectionId == connectionId)
            .ToListAsync();
    }

    public async Task DeleteAsync(
        string roleId,
        Guid connectionId)
    {
        var existing =
            await _context
                .RoleRuntimePermissions
                .Where(x =>
                    x.RoleId == roleId &&
                    x.ConnectionId == connectionId)
                .ToListAsync();

        _context
            .RoleRuntimePermissions
            .RemoveRange(existing);

        await _context.SaveChangesAsync();
    }
}