using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class ConnectionExclusionRepository
    : IConnectionExclusionRepository
{
    private readonly AppDbContext _context;

    public ConnectionExclusionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        List<ConnectionExclusion> entities)
    {
        await _context.ConnectionExclusions
            .AddRangeAsync(entities);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ConnectionExclusion>>
        GetByConnectionAsync(Guid connectionId)
    {
        return await _context.ConnectionExclusions
            .Where(x => x.ConnectionId == connectionId)
            .ToListAsync();
    }

    public async Task DeleteByConnectionAsync(
        Guid connectionId)
    {
        var existing = await _context.ConnectionExclusions
            .Where(x => x.ConnectionId == connectionId)
            .ToListAsync();

        _context.ConnectionExclusions.RemoveRange(existing);

        await _context.SaveChangesAsync();
    }
}