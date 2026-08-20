namespace AIChatbot.Domain.Entities;

public class ExternalUserMapping
{
    public Guid Id { get; set; }

    public string ApplicationUserId { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public string ExternalTable { get; set; } = default!;

    public string ExternalUserId { get; set; } = default!;

    public string MatchColumn { get; set; } = default!;

    public string MatchValue { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    public ConnectionString Connection { get; set; } = default!;
}