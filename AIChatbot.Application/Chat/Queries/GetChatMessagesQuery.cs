using MediatR;
using AIChatbot.Application.Chat.Results;

namespace AIChatbot.Application.Chat.Queries;


/// Query to retrieve all messages in a chat session

public record GetChatMessagesQuery(
    string UserId,
    Guid ChatSessionId
) : IRequest<IReadOnlyList<ChatMessageResult>>;
