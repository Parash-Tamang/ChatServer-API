using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Queries;
using AIChatbot.Application.Auth.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        var result = await _auth.GetProfileAsync(request.UserId);

        if (result == null)
            throw new KeyNotFoundException("User not found.");

        return result;
    }
}