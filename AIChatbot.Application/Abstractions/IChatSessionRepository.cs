using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;


/// Repository abstraction for chat sessions and messages

public interface IChatSessionRepository
{
    /// Retrieves a specific message by id

    Task<Message?> GetMessageAsync(Guid messageId);

   
    /// Creates a new chat session for the given user
   
    Task<Guid> CreateChatSessionAsync(string userId);

    
    /// Persists a chat message
   
    Task SaveMessageAsync(
        Guid chatSessionId,
        string role,
        string content);

    
    /// Checks whether a chat session belongs to the user
    
    Task<bool> ChatSessionBelongsToUser(
        Guid chatSessionId,
        string userId);

  
    /// Retrieves all messages for a chat session
   
    Task<IReadOnlyList<Message>> GetMessagesAsync(
        Guid chatSessionId);

    
    /// Retrieves all chat sessions for a user
  
    Task<IReadOnlyList<ChatSession>> GetAllSessionsAsync(
        string userId);

   
    /// Retrieves the latest N messages for a chat session
   
    Task<IReadOnlyList<Message>> GetLatestMessagesAsync(
        Guid chatSessionId,
        int count);

   
    /// Deletes a chat session and its messages
  
    Task DeleteChatSessionAsync(
        Guid chatSessionId);
}
