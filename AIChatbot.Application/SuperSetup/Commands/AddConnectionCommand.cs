using MediatR;
using AIChatbot.Application.SuperSetup.DTOs;
namespace AIChatbot.Application.SuperSetup.Commands;

public record AddConnectionCommand(
    string Server,
    string DatabaseName,
    string AuthMode,
    string? Username,
    string? Password,
    bool TrustCertificate,
    int ConnectionTimeout,
    string DbIdentifier,
    List<PromptFunctionDto> PromptFunctions,
    string RequestedByUserId
) : IRequest<Guid>;