using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class ExternalUserMappingRepository
    : IExternalUserMappingRepository
{
    private readonly AppDbContext _context;

    public ExternalUserMappingRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ExternalUserMapping entity)
    {
        _context.ExternalUserMappings.Add(entity);

        await _context.SaveChangesAsync();
    }

    public async Task<ExternalUserMapping?> GetAsync(
        string applicationUserId,
        Guid connectionId)
        
    {
        return await _context
            .ExternalUserMappings
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId ==
                    applicationUserId
                &&
                x.ConnectionId ==
                    connectionId);
    }
}