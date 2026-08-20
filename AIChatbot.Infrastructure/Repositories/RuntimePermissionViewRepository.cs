using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RuntimePermissionViewRepository
    : IRuntimePermissionViewRepository
{
    private readonly AppDbContext _context;

    public RuntimePermissionViewRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RuntimePermissionView entity)
    {
        _context.RuntimePermissionViews.Add(entity);

        await _context.SaveChangesAsync();
    }

    public async Task<RuntimePermissionView?> GetAsync(
        string roleId,
        Guid connectionId)
    {
        return await _context
            .RuntimePermissionViews
            .FirstOrDefaultAsync(x =>
                x.RoleId == roleId
                &&
                x.ConnectionId == connectionId);
    }

    public async Task DeleteAsync(
        RuntimePermissionView entity)
    {
        _context.RuntimePermissionViews.Remove(entity);

        await _context.SaveChangesAsync();
    }
}