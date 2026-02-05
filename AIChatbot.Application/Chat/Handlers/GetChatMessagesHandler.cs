using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Chat.Results;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;


/// Retrieves all messages for a chat session

public class GetChatMessagesHandler
    : IRequestHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageResult>>
{
    private readonly IChatSessionRepository _repo;

    public GetChatMessagesHandler(IChatSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<ChatMessageResult>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // Security check
        var ownsSession = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!ownsSession)
            throw new UnauthorizedAccessException();

        var messages = await _repo.GetMessagesAsync(request.ChatSessionId);

        return messages
            .Select(m => new ChatMessageResult
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToList();
    }
}
