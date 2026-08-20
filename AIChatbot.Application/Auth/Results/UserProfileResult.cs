namespace AIChatbot.Application.Auth.Results;

public class UserProfileResult
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;

    public IList<string>? Roles { get; init; }
}
