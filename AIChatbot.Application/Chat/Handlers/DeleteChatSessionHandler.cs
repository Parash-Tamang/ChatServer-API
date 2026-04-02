using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;

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
        var owns = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException("You do not have access to this session.");

        await _repo.DeleteChatSessionAsync(request.ChatSessionId);

        return Unit.Value;
    }
}