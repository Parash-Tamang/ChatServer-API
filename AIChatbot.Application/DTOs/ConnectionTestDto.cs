namespace AIChatbot.Application.DTOs;

public class ConnectionTestDto
{
    public string Server { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string AuthMode { get; set; } = default!; // sql | windows
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool TrustCertificate { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
    public string DbId { get; set; } = default!;
    public string? Role { get; set; }
}