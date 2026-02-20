namespace AIChatbot.Domain.Entities;

public class ResponseMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }
    public Message Message { get; set; } = default!;

    public bool Success { get; set; }
    public string? Query { get; set; }
    public string? InfoMessage { get; set; }

    public string? SqlGenerated { get; set; }

    public string? ColumnsJson { get; set; }
    public string? RowsJson { get; set; }

    public int RowCount { get; set; }
    public bool WasReconstructed { get; set; }
    public bool ClarificationNeeded { get; set; }

    public string? TokenUsageJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
