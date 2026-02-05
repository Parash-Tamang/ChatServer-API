using MediatR;

namespace AIChatbot.Application.Chat.Commands;


/// Command to delete a chat session belonging to a user

public record DeleteChatSessionCommand(
    string UserId,
    Guid ChatSessionId
) : IRequest<Unit>;
