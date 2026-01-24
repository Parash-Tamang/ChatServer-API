using AIChatbot.Application.DTOs;
namespace AIChatbot.Application.Interfaces;
public interface IChatSessionRepository
{
    Task<Guid> CreateChatSessionAsync(string userId);
    Task SaveMessageAsync(Guid chatSessionId, string role, string content);
    Task<bool> ChatSessionBelongsToUser(Guid chatSessionId, string userId);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatSessionId);
    Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync(string userId);
    Task DeleteChatSessionAsync(Guid chatSessionId);
}
