namespace AIChatbot.Domain.Entities;

public class ResponseMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }
    public Message Message { get; set; } = default!;

    // DB context used for execution
    public Guid? ConnectionStringId { get; set; }
    public ConnectionString? ConnectionString { get; set; }

    // Prompt context
    public Guid? PromptSetId { get; set; }

    // Role used while querying
    public string? RoleUsed { get; set; }

    // Core execution summary
    public bool Success { get; set; }
    public string? Query { get; set; }
    public string? InfoMessage { get; set; }
    public string? SqlGenerated { get; set; }
    public int RowCount { get; set; }
    public bool WasReconstructed { get; set; }
    public bool ClarificationNeeded { get; set; }

    // Full LLM reasoning stored as JSON
    public string? LlmResponseJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}