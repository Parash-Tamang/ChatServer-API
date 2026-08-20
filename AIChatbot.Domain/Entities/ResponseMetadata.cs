namespace AIChatbot.Domain.Entities;

public class ResponseMetadata
{
    public Guid Id { get; set; }
        = Guid.NewGuid();

    // =========================================================
    // MESSAGE
    // =========================================================

    public Guid MessageId { get; set; }

    public Message Message { get; set; }
        = default!;

    // =========================================================
    // DATABASE
    // =========================================================

    public Guid? ConnectionStringId { get; set; }

    public ConnectionString? ConnectionString { get; set; }

    // =========================================================
    // PROMPT / ROLE
    // =========================================================

    public Guid? PromptSetId { get; set; }

    public string? RoleUsed { get; set; }

    // =========================================================
    // QUERY
    // =========================================================

    public bool Success { get; set; }

    public string? Query { get; set; }

    public string? RefinedQuery { get; set; }

    public string? IntentDetected { get; set; }

    public string? InfoMessage { get; set; }

    public string? SqlGenerated { get; set; }
    public bool ExcelGenerated { get; set; }

    public int RowCount { get; set; }

    public bool WasReconstructed { get; set; }

    public bool ClarificationNeeded { get; set; }

    // =========================================================
    // TABLES / FILTERS
    // =========================================================

    public string? TablesUsedJson { get; set; }

    public string? FiltersJson { get; set; }

    // =========================================================
    // RESULT DATA
    // =========================================================

    public string? ColumnsJson { get; set; }

    public string? RowsJson { get; set; }

    // =========================================================
    // SESSION CONTEXT
    // =========================================================

    public string? SessionContextJson { get; set; }

    // =========================================================
    // GRAPH
    // =========================================================

    public string? GraphType { get; set; }

    public string? GraphTitle { get; set; }

    public string? GraphReasoning { get; set; }

    public string? GraphImageUrl { get; set; }

    public string? GraphImagePath { get; set; }

    // =========================================================
    // FULL RAW RESPONSE
    // =========================================================

    public string? LlmResponseJson { get; set; }

    public string? FullResponseJson { get; set; }

    // =========================================================
    // TIMESTAMP
    // =========================================================

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}