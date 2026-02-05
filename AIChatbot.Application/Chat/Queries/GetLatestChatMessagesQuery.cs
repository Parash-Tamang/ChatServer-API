using MediatR;
using AIChatbot.Application.Chat.Results;

namespace AIChatbot.Application.Chat.Queries;


/// Query to retrieve the latest N messages from a chat session

public record GetLatestChatMessagesQuery(
    string UserId,
    Guid ChatSessionId,
    int Count
) : IRequest<IReadOnlyList<ChatMessageResult>>;
