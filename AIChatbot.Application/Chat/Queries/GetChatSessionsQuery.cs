using MediatR;
using AIChatbot.Application.Chat.Results;

namespace AIChatbot.Application.Chat.Queries;


/// Query to retrieve all chat sessions for a user

public record GetChatSessionsQuery(
    string UserId
) : IRequest<IReadOnlyList<ChatSessionSummaryResult>>;
