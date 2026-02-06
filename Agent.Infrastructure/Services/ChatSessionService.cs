using Agent.Application.Dto.UserManagement;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using Agent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agent.Infrastructure.Services
{
    public class ChatSessionService : IChatSessionService
    {
        private readonly ApplicationDbContext _DbContext;
        public ChatSessionService(ApplicationDbContext DbContext)
        {

            _DbContext = DbContext;
        }

        private static string BuildTitle(string message)
        {
            var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length <= 4
                ? string.Join(' ', words)
                : string.Join(' ', words.Take(4));
        }

        public async Task<UserSession> CreateChatSessionAsync(string message, string userId)
        {
            var session = new UserSession
            {
                UserId = userId,
                Title = BuildTitle(message)
            };

            await _DbContext.UserSessions.AddAsync(session);
            await _DbContext.SaveChangesAsync();


            return session;

        }

        public async Task<UserSession?> GetExistAsync(string sessionId)
        {

            return await _DbContext.UserSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId);
        }


        public async Task<List<UserMessageResponseDto>> GetChatMessageBySessionId(string sessionId)
        {
            var messages = await _DbContext.UserMessages.
                Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new UserMessageResponseDto
            {
                MessageId = m.MessageId.ToString(),
                SessionId = m.SessionId.ToString(),
                SenderType = m.SenderType,          // "user" | "assistant"
                MessageText = m.MessageText,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

            return messages;
        }

        //public async Task<SessionTitleResponseDto> GetUserSessionsAsync(string userId)
        //{
        //    var sessions = await _DbContext.UserSessions
        //.Where(s => s.UserId == userId)
        //.Select(s => new ChatResponseDto
        //{
        //    SessionId = s.SessionId.ToString(),
        //    SessionTitle = s.Title
        //})
        //.ToListAsync();

        //    return new SessionTitleResponseDto
        //    {
        //        sessionTitleList = sessions
        //    };
        //}

        public async Task<SessionTitleResponseDto> GetUserSessionsAsync(string userId)
        {
            var sessions = await _DbContext.UserSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt) // ✅ Sort newest first at database level
                .Select(s => new ChatResponseDto
                {
                    SessionId = s.SessionId.ToString(),
                    SessionTitle = s.Title,
                    CreatedAt = s.CreatedAt // ✅ Include timestamp for client-side display
                })
                .ToListAsync();

            return new SessionTitleResponseDto
            {
                sessionTitleList = sessions
            };
        }

        public async Task<List<UserMessageResponseDto>> GetLastChatMessagesAsync(string sessionId)
        {
            var messages = await _DbContext.UserMessages
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)   // newest first
                .Take(5)                              // last 5 only
                .OrderBy(m => m.CreatedAt)             // reorder oldest → newest
                .Select(m => new UserMessageResponseDto
                {
                    MessageId = m.MessageId.ToString(),
                    SessionId = m.SessionId.ToString(),
                    SenderType = m.SenderType,
                    MessageText = m.MessageText,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return messages;
        }

    }
}

