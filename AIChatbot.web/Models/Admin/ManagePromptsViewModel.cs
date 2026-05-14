using AIChatbot.web.Dto;

namespace AIChatbot.web.Models.Admin
{
    public class ManagePromptsViewModel
    {
        public List<ConnectionRequestDto> Connections { get; set; } = new();
        public List<GlobalPromptDto> GlobalPrompts { get; set; } = new();
        public Dictionary<string, List<LocalPromptDto>> LocalPrompts { get; set; } = new();
    }

    public class GlobalPromptDto
    {
        public string Id { get; set; }
        public string FunctionName { get; set; }
        public string SystemPrompt { get; set; }
    }

    public class LocalPromptDto
    {
        public string Id { get; set; }
        public string FunctionName { get; set; }
        public string SystemPrompt { get; set; }
    }
}
public class SetPromptingModeRequest
{
    public string ConnectionId { get; set; }
    public int PromptingMode { get; set; }
}
public class SaveGlobalPromptRequest
{
    public string? FunctionId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
}

public class SaveLocalPromptRequest
{
    public string ConnectionStringId { get; set; } = string.Empty;
    public string? FunctionId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
}