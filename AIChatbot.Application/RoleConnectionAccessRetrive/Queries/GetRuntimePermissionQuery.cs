using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetRuntimePermissionQuery(
    string RoleId,
    Guid ConnectionId)
    : IRequest<RuntimePermissionResponseDto?>;