namespace AIChatbot.Application.Supersetup.DTOs;

public class ConnectionDto
{
    public Guid Id { get; set; }
    public string ServerName { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string AuthMode { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool Verified { get; set; }
}