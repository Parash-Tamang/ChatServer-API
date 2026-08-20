using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RoleConnectionUserLookupConfigurationRepository
    : IRoleConnectionUserLookupConfigurationRepository
{
    private readonly AppDbContext _context;

    public RoleConnectionUserLookupConfigurationRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        RoleConnectionUserLookupConfiguration entity)
    {
        _context
            .RoleConnectionUserLookupConfigurations
            .Add(entity);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        RoleConnectionUserLookupConfiguration entity)
    {
        _context
            .RoleConnectionUserLookupConfigurations
            .Update(entity);

        await _context.SaveChangesAsync();
    }

    public async Task<RoleConnectionUserLookupConfiguration?>
        GetAsync(
            string roleId,
            Guid connectionId)
    {
        return await _context
            .RoleConnectionUserLookupConfigurations
            .FirstOrDefaultAsync(x =>
                x.RoleId == roleId &&
                x.ConnectionId == connectionId);
    }

    public async Task DeleteAsync(
        string roleId,
        Guid connectionId)
    {
        var existing = await _context
            .RoleConnectionUserLookupConfigurations
            .FirstOrDefaultAsync(x =>
                x.RoleId == roleId &&
                x.ConnectionId == connectionId);

        if (existing != null)
        {
            _context
                .RoleConnectionUserLookupConfigurations
                .Remove(existing);

            await _context.SaveChangesAsync();
        }
    }
}