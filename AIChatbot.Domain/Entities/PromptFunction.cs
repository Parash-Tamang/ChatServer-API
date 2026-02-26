namespace AIChatbot.Domain.Entities;

public class PromptFunction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PromptSetId { get; set; }
    public PromptSet PromptSet { get; set; } = default!;

    public string FunctionName { get; set; } = default!;
    public string SystemPrompt { get; set; } = default!;
}