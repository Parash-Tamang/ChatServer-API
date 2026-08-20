namespace AIChatbot.Api.Models.Agent;

public class GetTableRuntimeRequest
{
    public string UserId { get; set; }
        = string.Empty;

    public List<string> TableNames { get; set; }
        = new();
}
