



using MediatR;
using AIChatbot.Application.Supersetup.DTOs;
namespace AIChatbot.Application.Supersetup.Queries;
public record GetGlobalFunctionNamesQuery()
    : IRequest<List<FunctionDto>>;