namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class SaveRuntimePermissionRequest
{
    public string Role { get; set; } = string.Empty;

    public string SelectedRoleId { get; set; } = string.Empty;

    public string Database { get; set; } = string.Empty;

    public Guid ConnectionId { get; set; }

    public Dictionary<string, RuntimeTablePermissionDto>
        Permissions
    { get; set; } = new();
}
