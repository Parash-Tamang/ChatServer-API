using MediatR;
using AIChatbot.Application.Chat.Results;

namespace AIChatbot.Application.Chat.Commands;


/// Command to send a user message to the chat session
/// If ChatSessionId is null, a new session will be created

public record SendChatMessageCommand(
    string UserId,
    Guid? ChatSessionId,
    string Message
) : IRequest<ChatExecutionResult>;
