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

   

    public bool Verified { get; set; } = false;

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false;  // for soft delete
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PromptFunction> Functions { get; set; } = new List<PromptFunction>();
   // public ICollection<RoleDbPermission> RoleDbPermissions { get; set; } = new List<RoleDbPermission>();
}