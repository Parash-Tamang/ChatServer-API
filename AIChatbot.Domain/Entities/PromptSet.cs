namespace AIChatbot.Domain.Entities;

public class PromptSet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConnectionStringId { get; set; }
    public ConnectionString ConnectionString { get; set; } = default!;

    public string VersionType { get; set; } = default!; // ORIGINAL / CURRENT
    public int VersionNumber { get; set; }

    public string CreatedBy { get; set; } = default!; // SA/Admin id
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PromptFunction> Functions { get; set; } = new List<PromptFunction>();
}