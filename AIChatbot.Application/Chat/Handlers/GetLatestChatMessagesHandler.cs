using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Chat.Results;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;


/// Retrieves the latest N messages from a chat session

public class GetLatestChatMessagesHandler
    : IRequestHandler<GetLatestChatMessagesQuery, IReadOnlyList<ChatMessageResult>>
{
    private readonly IChatSessionRepository _repo;

    public GetLatestChatMessagesHandler(IChatSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<ChatMessageResult>> Handle(
        GetLatestChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var owns = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException();

        var messages = await _repo.GetLatestMessagesAsync(
            request.ChatSessionId,
            request.Count);

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
