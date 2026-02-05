using MediatR;
using AIChatbot.Application.Chat.Results;

namespace AIChatbot.Application.Chat.Commands;


/// Command to retry generating an assistant reply for a given message

public record RetryAssistantReplyCommand(
    string UserId,
    Guid ChatSessionId,
    Guid MessageId
) : IRequest<ChatCommandResult>;
