using AIChatbot.Domain.Entities;

public class PromptFunction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ✅ MUST be nullable (we fixed this earlier)
    public Guid? ConnectionStringId { get; set; }

    public ConnectionString? ConnectionString { get; set; }

    public bool IsDeleted { get; set; }

    public string FunctionName { get; set; } = default!;
    public string SystemPrompt { get; set; } = default!;
}