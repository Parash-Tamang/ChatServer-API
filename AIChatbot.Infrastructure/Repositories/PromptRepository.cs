using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class PromptRepository : IPromptRepository
{
    private readonly AppDbContext _db;

    public PromptRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PromptSet?> GetCurrentPromptAsync(Guid dbId)
        => await _db.PromptSets
            .FirstOrDefaultAsync(x => x.ConnectionStringId == dbId && x.VersionType == "CURRENT");

    public async Task<PromptSet?> GetOriginalPromptAsync(Guid dbId)
        => await _db.PromptSets
            .FirstOrDefaultAsync(x => x.ConnectionStringId == dbId && x.VersionType == "ORIGINAL");

    public async Task<List<PromptFunction>> GetFunctionsAsync(Guid promptSetId)
        => await _db.PromptFunctions.Where(x => x.PromptSetId == promptSetId).ToListAsync();

    public async Task AddPromptSetAsync(PromptSet set)
    {
        _db.PromptSets.Add(set);
        await _db.SaveChangesAsync();
    }

    public async Task AddFunctionsAsync(List<PromptFunction> functions)
    {
        _db.PromptFunctions.AddRange(functions);
        await _db.SaveChangesAsync();
    }

    public async Task ReplaceFunctionsAsync(Guid promptSetId, List<PromptFunction> functions)
    {
        var old = _db.PromptFunctions.Where(x => x.PromptSetId == promptSetId);
        _db.PromptFunctions.RemoveRange(old);

        _db.PromptFunctions.AddRange(functions);
        await _db.SaveChangesAsync();
    }
}