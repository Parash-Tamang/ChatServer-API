using System.Text.Json.Serialization;

namespace AIChatbot.Application.Auth.Results;

public sealed class AuthResult
{
    public bool Success { get; set; }

  

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public int ExpiresIn { get; set; }

    public string? Error { get; set; }

    [JsonIgnore]
    public IList<string>? Roles { get; set; }

    public string? Role => Roles?.FirstOrDefault();
}