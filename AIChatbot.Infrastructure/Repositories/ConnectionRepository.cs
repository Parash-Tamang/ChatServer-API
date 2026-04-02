using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly AppDbContext _context;

    public ConnectionRepository(AppDbContext context)
    {
        _context = context;
    }

    // Get all non-deleted connections
    public async Task<List<ConnectionString>> GetAllAsync()
    {
        return await _context.ConnectionStrings
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }

    // Get connection by Id
    public async Task<ConnectionString?> GetByIdAsync(Guid id)
    {
        return await _context.ConnectionStrings
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    // Add new connection
    public async Task AddAsync(ConnectionString entity)
    {
        _context.ConnectionStrings.Add(entity);
        await _context.SaveChangesAsync();
    }

    // Update connection
    public async Task UpdateAsync(ConnectionString entity)
    {
        _context.ConnectionStrings.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Set one connection active (others inactive)
    public async Task SetActiveAsync(Guid id)
    {
        var allConnections = await _context.ConnectionStrings
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        foreach (var conn in allConnections)
        {
            conn.IsActive = false;
        }

        var target = allConnections.FirstOrDefault(x => x.Id == id);

        if (target == null)
            throw new Exception("Connection not found");

        target.IsActive = true;

        await _context.SaveChangesAsync();
    }
    public async Task<ConnectionString?> GetByUniqueKeyAsync(
    string serverName,
    string databaseName,
    string authMode)
    {
        return await _context.ConnectionStrings
            .Where(x =>
                x.ServerName == serverName &&
                x.DatabaseName == databaseName &&
                x.AuthMode == authMode &&
                !x.IsDeleted)
            .FirstOrDefaultAsync();
    }
}