namespace AIChatbot.Application.Supersetup.DTOs;

public class FunctionDto
{
    public Guid Id { get; set; }

    public string FunctionName { get; set; } = default!;

    public string SystemPrompt { get; set; } = default!; // 🔥 final resolved prompt

    public string Source { get; set; } = string.Empty;   // "local" or "global"
}