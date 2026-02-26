namespace AIChatbot.Application.RoleAccess.Models;

public class RoleAccessResult
{
    public List<Guid> Databases { get; set; } = new();

    // DB → tables mapping
    public Dictionary<Guid, List<string>> Tables { get; set; } = new();

    // DB → Table → Columns mapping
    public Dictionary<Guid, Dictionary<string, List<string>>> Columns { get; set; } = new();
}