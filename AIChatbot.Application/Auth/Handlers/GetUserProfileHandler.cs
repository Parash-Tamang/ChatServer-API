using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Queries;
using AIChatbot.Application.Auth.Results;
using MediatR;

public class GetUserProfileHandler
    : IRequestHandler<GetUserProfileQuery, UserProfileResult>
{
    private readonly IAuthService _auth;

    public GetUserProfileHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<UserProfileResult> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        return await _auth.GetProfileAsync(request.UserId);
    }
}
