using MediatR;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Supersetup.Commands;

public record SetPromptCommand(
    Guid DbId,
    List<PromptFunction> Functions,
    string RequestedByUserId
) : IRequest<bool>;