
using MediatR;
using AIChatbot.Application.Supersetup.DTOs;
namespace AIChatbot.Application.Supersetup.Commands;


public record SaveGlobalPromptCommand(
     Guid? FunctionId,
    string FunctionName,
    string SystemPrompt
) : IRequest<FunctionDto>;