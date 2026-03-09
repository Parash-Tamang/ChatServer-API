using MediatR;
using AIChatbot.Application.Supersetup.DTOs;

namespace AIChatbot.Application.Supersetup.Commands
{
    public record DeleteSupersetupCommand(
        Guid? ConnectionId,
        Guid? FunctionId
    ) : IRequest<bool>;
}