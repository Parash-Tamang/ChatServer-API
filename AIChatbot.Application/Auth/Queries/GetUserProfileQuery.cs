using AIChatbot.Application.Auth.Results;
using MediatR;
namespace AIChatbot.Application.Auth.Queries;

public record GetUserProfileQuery(string UserId)
    : IRequest<UserProfileResult>;
