using MediatR;

namespace AIChatbot.Application.Supersetup.Commands;

public record UpdateConnectionCommand(
    Guid DbId,
    string Server,
    string Database,
    string AuthMode,
    string? Username,
    string? Password,
    bool TrustCertificate,
    int Timeout,
    string RequestedByUserId
) : IRequest<bool>;