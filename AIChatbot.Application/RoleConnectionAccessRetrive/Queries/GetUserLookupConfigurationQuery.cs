using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetUserLookupConfigurationQuery(
    string RoleId,
    Guid ConnectionId)
    : IRequest<RoleConnectionUserLookupConfiguration?>;