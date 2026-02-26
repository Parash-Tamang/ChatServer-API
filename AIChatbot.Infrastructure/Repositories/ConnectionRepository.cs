using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly AppDbContext _db;

    public ConnectionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ConnectionString?> GetByIdAsync(Guid id)
        => await _db.ConnectionStrings.FindAsync(id);

    public async Task<List<ConnectionString>> GetAllAsync()
        => await _db.ConnectionStrings.ToListAsync();

    public async Task AddAsync(ConnectionString entity)
    {
        _db.ConnectionStrings.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ConnectionString entity)
    {
        _db.ConnectionStrings.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ConnectionString entity)
    {
        _db.ConnectionStrings.Remove(entity);
        await _db.SaveChangesAsync();
    }
}