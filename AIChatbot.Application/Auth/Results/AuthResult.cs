namespace AIChatbot.Application.Auth.Results;

public sealed class AuthResult
{
    public bool Success { get; init; }

    public string? UserId { get; init; }

    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }

    public int ExpiresIn { get; init; }

    public string? Error { get; init; }
    public IList<string>? Roles { get; init; }
}
