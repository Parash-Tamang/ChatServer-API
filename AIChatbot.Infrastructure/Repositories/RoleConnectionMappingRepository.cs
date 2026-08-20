using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RoleConnectionMappingRepository
    : IRoleConnectionMappingRepository
{
    private readonly AppDbContext _context;

    public RoleConnectionMappingRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RoleConnectionMapping entity)
    {
        _context.RoleConnectionMappings.Add(entity);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        string roleId,
        Guid connectionId)
    {
        var entity =
            await _context.RoleConnectionMappings
                .FirstOrDefaultAsync(x =>
                    x.RoleId == roleId &&
                    x.ConnectionId == connectionId);

        if (entity == null)
            return;

        _context.RoleConnectionMappings.Remove(entity);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(
        string roleId,
        Guid connectionId)
    {
        return await _context.RoleConnectionMappings
            .AnyAsync(x =>
                x.RoleId == roleId &&
                x.ConnectionId == connectionId);
    }

    public async Task<List<RoleConnectionMapping>>
        GetByRoleIdAsync(string roleId)
    {
        return await _context.RoleConnectionMappings
            .Where(x => x.RoleId == roleId)
            .Include(x => x.Connection)
            .Include(x => x.Role)
            .ToListAsync();
    }

    public async Task<List<RoleConnectionMapping>>
        GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.RoleConnectionMappings
            .Where(x => x.ConnectionId == connectionId)
            .Include(x => x.Connection)
            .Include(x => x.Role)
            .ToListAsync();
    }
}