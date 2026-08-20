using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetConnectionExclusionsQuery(
    Guid ConnectionId)
    : IRequest<List<string>>;