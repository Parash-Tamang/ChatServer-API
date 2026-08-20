using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetRoleConnectionsQuery(
    string RoleId)
    : IRequest<List<RoleConnectionDto>>;