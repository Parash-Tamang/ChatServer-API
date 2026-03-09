using MediatR;
using AIChatbot.Application.Supersetup.DTOs;
using System.Text.Json.Serialization;
using AIChatbot.Application.Json;

namespace AIChatbot.Application.Supersetup.Commands
{
    public record SaveConnectionCommand(
        Guid? Id,
        string ServerName,
        string DatabaseName,
        string AuthMode,
        string? Username,
        string? Password,
        bool TrustCertificate,
        int ConnectionTimeout,
        bool IsActive
    ) : IRequest<List<ConnectionDto>>;
}