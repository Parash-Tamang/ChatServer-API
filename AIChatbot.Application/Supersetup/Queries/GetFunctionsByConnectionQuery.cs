using MediatR;
using AIChatbot.Application.Supersetup.DTOs;

namespace AIChatbot.Application.Supersetup.Queries;

public record GetFunctionsByConnectionQuery(Guid ConnectionId)
    : IRequest<List<FunctionDto>>;