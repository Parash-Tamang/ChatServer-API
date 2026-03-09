using MediatR;
using AIChatbot.Application.Supersetup.DTOs;

namespace AIChatbot.Application.Supersetup.Commands
{
    public record GetSupersetupDataQuery()
        : IRequest<SupersetupOverviewDto>;
}