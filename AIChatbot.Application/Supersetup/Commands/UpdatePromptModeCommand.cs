using MediatR;

namespace AIChatbot.Application.Supersetup.Commands
{
    public record UpdatePromptModeCommand(
        Guid ConnectionId,
        int PromptingMode // 0 or 1
    ) : IRequest<bool>;
}