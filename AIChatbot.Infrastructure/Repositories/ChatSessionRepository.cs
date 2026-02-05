using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;


// EF Core repository for chat sessions and messages

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _context;

    public ChatSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    // CREATE CHAT SESSION
  
    public async Task<Guid> CreateChatSessionAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        var chatSession = new ChatSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatSessions.Add(chatSession);
        await _context.SaveChangesAsync();

        return chatSession.Id;
    }

    // SAVE MESSAGE
  
    public async Task SaveMessageAsync(
        Guid chatSessionId,
        string role,
        string content)
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            Role = role,
            Content = content,
            // Explicit timestamp for clarity & consistency
            
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

   
    // SECURITY CHECK
 
    public async Task<bool> ChatSessionBelongsToUser(
        Guid chatSessionId,
        string userId)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == chatSessionId &&
                x.UserId == userId);
    }

   
    // LOAD ALL MESSAGES
  
    public async Task<IReadOnlyList<Message>> GetMessagesAsync(
        Guid chatSessionId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatSessionId == chatSessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

   
    // LOAD ALL SESSIONS
  
    public async Task<IReadOnlyList<ChatSession>> GetAllSessionsAsync(
        string userId)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

   
    // LOAD LATEST N MESSAGES
   
    public async Task<IReadOnlyList<Message>> GetLatestMessagesAsync(
        Guid chatSessionId,
        int count)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatSessionId == chatSessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt) // restore chronological order
            .ToListAsync();
    }

   
    // LOAD SINGLE MESSAGE
 
    public async Task<Message?> GetMessageAsync(Guid messageId)
    {
        return await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

   
    // DELETE SESSION
   
    public async Task DeleteChatSessionAsync(Guid chatSessionId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == chatSessionId);

        if (session == null)
            return;

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
    }
}
