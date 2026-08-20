using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Domain.Entities;

public class RuntimePermissionView
{
    public Guid Id { get; set; }

    public string RoleId { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public string ViewName { get; set; } = default!;
    public string ViewSql { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    // Optional runtime data returned from the view generation step.
    // This is used at runtime to supply values for runtime keys.
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Dictionary<string, object?>? RuntimeData { get; set; }

    // ============================================================
    // NAVIGATION
    // ============================================================

    public IdentityRole Role { get; set; } = default!;

    public ConnectionString Connection { get; set; } = default!;
}