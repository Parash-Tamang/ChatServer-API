using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;


/// Handles deletion of a chat session owned by a user

public class DeleteChatSessionHandler
    : IRequestHandler<DeleteChatSessionCommand, Unit>
{
    private readonly IChatSessionRepository _repo;

    public DeleteChatSessionHandler(IChatSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(
        DeleteChatSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Security: ensure session belongs to user
        var owns = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException();

        await _repo.DeleteChatSessionAsync(request.ChatSessionId);

        return Unit.Value;
    }
}
