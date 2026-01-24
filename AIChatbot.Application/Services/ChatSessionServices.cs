using AIChatbot.Application.DTOs;
using AIChatbot.Application.Interfaces;

namespace AIChatbot.Application.Services;

public class ChatSessionServices : IChatSessionService

{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;

    public ChatSessionServices(
        IChatSessionRepository repo,
        IAiProviderService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatSessionId)
    {
        return await _repo.GetMessagesAsync(chatSessionId);
    }

    public async Task<bool> ChatSessionBelongsToUserAsync(Guid chatSessionId, string userId)
    {
        return await _repo.ChatSessionBelongsToUser(chatSessionId, userId);
    }

    public async Task<IEnumerable<ChatSessionDto>> GetAllAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required");

        return await _repo.GetAllSessionsAsync(userId);
    }

    public async Task<ChatSendResult> SendAsync(
        string userId,
        Guid? chatSessionId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required");

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty");

        Guid sessionId;

        // 🔹 FIRST MESSAGE → CREATE NEW SESSION
        if (chatSessionId == null)
        {
            sessionId = await _repo.CreateChatSessionAsync(userId);
        }
        else
        {
            // 🔹 SECURITY CHECK
            var ownsSession =
                await _repo.ChatSessionBelongsToUser(chatSessionId.Value, userId);

            if (!ownsSession)
                throw new UnauthorizedAccessException("Invalid chat session");

            sessionId = chatSessionId.Value;
        }

        // 🔹 SAVE USER MESSAGE
        await _repo.SaveMessageAsync(
            sessionId,
            role: "user",
            content: message
        );

        // 🔹 LOAD CONTEXT FOR AI
        var messages = await _repo.GetMessagesAsync(sessionId);

        // 🔹 AI RESPONSE (DTO-BASED, CLEAN)
        var reply = await _ai.GetReplyAsync(messages);

        // 🔹 SAVE AI MESSAGE
        await _repo.SaveMessageAsync(
            sessionId,
            role: "assistant",
            content: reply
        );

        return new ChatSendResult
        {
            ChatSessionId = sessionId,
            Reply = reply
        };
    }

    public async Task DeleteAsync(Guid chatSessionId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required");

        var ownsSession =
            await _repo.ChatSessionBelongsToUser(chatSessionId, userId);

        if (!ownsSession)
            throw new UnauthorizedAccessException("Invalid chat session");

        await _repo.DeleteChatSessionAsync(chatSessionId);
    }
}
