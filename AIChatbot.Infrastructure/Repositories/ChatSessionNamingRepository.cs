using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;

public class ChatSessionNamingRepository : IChatSessionNamingRepository
{
    private readonly AppDbContext _db;

    public ChatSessionNamingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(ChatSessionNaming naming)
    {
        _db.ChatSessionNaming.Add(naming);
        await _db.SaveChangesAsync();
    }
}
