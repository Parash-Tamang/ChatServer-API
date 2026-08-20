using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetDatabaseSchemaQuery(
    Guid ConnectionId)
    : IRequest<List<DatabaseSchemaDto>>;