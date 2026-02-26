namespace AIChatbot.Domain.Entities;

public class ConnectionString
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ServerName { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string AuthMode { get; set; } = default!;
    public string? Username { get; set; }
    public string? PasswordEncrypted { get; set; }

    public bool TrustCertificate { get; set; }
    public int ConnectionTimeout { get; set; }

    public string DbIdentifier { get; set; } = default!;
    public string? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PromptSet> PromptSets { get; set; } = new List<PromptSet>();
    public ICollection<RoleDbPermission> RoleDbPermissions { get; set; } = new List<RoleDbPermission>();
}