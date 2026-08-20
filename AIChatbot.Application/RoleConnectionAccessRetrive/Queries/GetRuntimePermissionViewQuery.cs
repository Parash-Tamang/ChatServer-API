using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetRuntimePermissionViewQuery(
    string RoleId,
    Guid ConnectionId)
    : IRequest<RuntimePermissionViewDto?>;