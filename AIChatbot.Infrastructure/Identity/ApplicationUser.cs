using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Infrastructure.Identity;


/// Application user entity extending ASP.NET Identity user

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
