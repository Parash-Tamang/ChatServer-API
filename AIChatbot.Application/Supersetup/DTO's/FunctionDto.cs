namespace AIChatbot.Application.Supersetup.DTOs;

public class FunctionDto
{
    public Guid Id { get; set; }
    public string FunctionName { get; set; } = default!;
    public string SystemPrompt { get; set; } = default!;
}