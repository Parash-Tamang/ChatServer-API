using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AIChatbot.Application.DTOs;
using AIChatbot.Application.Interfaces;
using AIChatbot.Infrastructure.Data;

namespace AIChatbot.Infrastructure.Repositories;

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _context;

    public ChatSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    // ================================
    // CREATE CHAT SESSION
    // ================================
    public async Task<Guid> CreateChatSessionAsync(string userId)
    {
        await using var conn = new SqlConnection(
            _context.Database.GetConnectionString()
        );
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "CreateChatSession";
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add(
            new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = userId }
        );

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            throw new InvalidOperationException("CreateChatSession returned no result");

        var success = reader.GetInt32(0);

        if (success == 0)
            throw new InvalidOperationException(reader.GetString(1));

        return reader.GetGuid(1);
    }

    // ================================
    // SAVE MESSAGE
    // ================================
    public async Task SaveMessageAsync(Guid chatSessionId, string role, string content)
    {
        await using var conn = new SqlConnection(
            _context.Database.GetConnectionString()
        );
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SaveMessage";
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add(new SqlParameter("@ChatSessionId", SqlDbType.UniqueIdentifier)
        {
            Value = chatSessionId
        });
        cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar, 50)
        {
            Value = role
        });
        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar)
        {
            Value = content
        });

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            throw new InvalidOperationException("SaveMessage returned no result");

        var success = reader.GetInt32(0);

        if (success == 0)
            throw new InvalidOperationException(reader.GetString(1));
    }

    // ================================
    // SECURITY CHECK
    // ================================
    public async Task<bool> ChatSessionBelongsToUser(Guid chatSessionId, string userId)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .AnyAsync(x => x.Id == chatSessionId && x.UserId == userId);
    }

    // ================================
    // LOAD MESSAGES
    // ================================
    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatSessionId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatSessionId == chatSessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    // ================================
    // LOAD ALL SESSIONS
    // ================================
    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync(string userId)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChatSessionDto
            {
                Id = c.Id,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    // ================================
    // DELETE SESSION
    // ================================
    public async Task DeleteChatSessionAsync(Guid chatSessionId)
    {
        var session = await _context.ChatSessions.FindAsync(chatSessionId);
        if (session == null)
            return;

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
    }
}
