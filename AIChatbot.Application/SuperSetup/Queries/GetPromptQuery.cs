using MediatR;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Supersetup.Queries;

public record GetPromptQuery(Guid DbId, string RequestedByUserId)
    : IRequest<PromptSet>;