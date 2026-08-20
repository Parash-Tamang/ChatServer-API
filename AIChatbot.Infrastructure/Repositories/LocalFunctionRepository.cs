using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class LocalFunctionRepository : ILocalFunctionRepository
{
    private readonly AppDbContext _context;

    public LocalFunctionRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task RemoveOverridesByFunctionName(string functionName)
    {
        var blocks = _context.LocalFunctionBlocks
            .Where(x => x.FunctionName == functionName);

        foreach (var b in blocks)
        {
            b.IsOverride = false;
            b.OverridePrompt = null;
            b.OverrideVersion = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteByConnectionIdAsync(Guid connectionId)
    {
        var blocks = _context.LocalFunctionBlocks
            .Where(x => x.ConnectionId == connectionId);

        _context.LocalFunctionBlocks.RemoveRange(blocks);
        await _context.SaveChangesAsync();
    }
    public async Task<LocalFunctionBlock?> GetAsync(Guid connectionId, string functionName)
    {
        return await _context.LocalFunctionBlocks
            .FirstOrDefaultAsync(x =>
                x.ConnectionId == connectionId &&
                x.FunctionName == functionName);
    }

    public async Task<List<LocalFunctionBlock>> GetByConnectionAsync(Guid connectionId)
    {
        return await _context.LocalFunctionBlocks
            .Where(x => x.ConnectionId == connectionId)
            .ToListAsync();
    }
    public async Task<LocalFunctionBlock?> GetByIdAsync(Guid id)
    {
        return await _context.LocalFunctionBlocks
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task AddRangeAsync(List<LocalFunctionBlock> blocks)
    {
        _context.LocalFunctionBlocks.AddRange(blocks);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LocalFunctionBlock block)
    {
        _context.LocalFunctionBlocks.Update(block);
        await _context.SaveChangesAsync();
    }
}