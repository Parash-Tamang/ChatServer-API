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
    public async Task<Guid> SaveMessageAsync(
       Guid chatSessionId,
       string role,
       string content)
    {
        if (chatSessionId == Guid.Empty)
            throw new ArgumentException("ChatSessionId cannot be empty");

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required");

        content ??= string.Empty;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            Role = role.Trim().ToLower(),   // normalize
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow     // 🔥 IMPORTANT
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return message.Id;
    }

    // get header of session 
    public async Task<string?> GetSessionTopicAsync(Guid chatSessionId)
    {
        var topic = await _context.ChatSessionNaming
            .AsNoTracking()
            .Where(x => x.ChatSessionId == chatSessionId)
            .Select(x => x.TopicName)
            .FirstOrDefaultAsync();

        return topic;
    }

    public async Task<IReadOnlyList<(Guid Id, DateTime CreatedAt, string? Topic)>>
      GetAllSessionsWithTopicAsync(string userId)

    {
        var result = await
            (from s in _context.ChatSessions.AsNoTracking()
             where s.UserId == userId
             join n in _context.ChatSessionNaming.AsNoTracking()
                on s.Id equals n.ChatSessionId into naming
             from n in naming.DefaultIfEmpty()
             orderby s.CreatedAt descending
             select new
             {
                 s.Id,
                 s.CreatedAt,
                 Topic = n.TopicName
             })
             .ToListAsync();

        return result
      .Select(x => (
          x.Id,
          x.CreatedAt,
          (string?)x.Topic   // 🔥 fix nullable
      ))
      .ToList();
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
