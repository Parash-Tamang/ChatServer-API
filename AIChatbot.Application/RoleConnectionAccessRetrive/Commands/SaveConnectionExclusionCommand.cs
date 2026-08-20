using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record SaveConnectionExclusionCommand(
    Guid ConnectionId,
    List<ConnectionExclusionDto> Exclusions)
    : IRequest<GenericResult>;