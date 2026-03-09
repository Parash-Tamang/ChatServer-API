using MediatR;
using AIChatbot.Application.Supersetup.DTOs;

namespace AIChatbot.Application.Supersetup.Commands
{
    public record SavePromptFunctionCommand(
        Guid? ConnectionStringId,
        Guid? FunctionId,
        string FunctionName,
        string SystemPrompt
    ) : IRequest<List<FunctionDto>>;
}