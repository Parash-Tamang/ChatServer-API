using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Chat.Results;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;

public class GetChatSessionsHandler
    : IRequestHandler<GetChatSessionsQuery, IReadOnlyList<ChatSessionSummaryResult>>
{
    private readonly IChatSessionRepository _repo;

    public GetChatSessionsHandler(IChatSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<ChatSessionSummaryResult>> Handle(
        GetChatSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _repo.GetAllSessionsWithTopicAsync(request.UserId);

        return sessions.Select(s => new ChatSessionSummaryResult
        {
            Id = s.Id,
            CreatedAt = s.CreatedAt,
            TopicName = string.IsNullOrWhiteSpace(s.Topic)
                ? "New Chat"
                : s.Topic
        }).ToList();
    }
}
