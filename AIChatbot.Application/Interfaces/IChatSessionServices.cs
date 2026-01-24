using AIChatbot.Application.DTOs;

namespace AIChatbot.Application.Interfaces;

public interface IChatSessionService
{
    Task<IEnumerable<ChatSessionDto>> GetAllAsync(string userId);

    Task<ChatSendResult> SendAsync(
        string userId,
        Guid? chatSessionId,
        string message
    );

    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatSessionId);
    Task<bool> ChatSessionBelongsToUserAsync(Guid chatSessionId, string userId);

    Task DeleteAsync(Guid chatSessionId, string userId);
}
