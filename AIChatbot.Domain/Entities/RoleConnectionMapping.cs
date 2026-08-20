using AIChatbot.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class RoleConnectionMapping
{
    public Guid Id { get; set; }

    public string RoleId { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public IdentityRole Role { get; set; } = default!;

    public ConnectionString Connection { get; set; } = default!;
}