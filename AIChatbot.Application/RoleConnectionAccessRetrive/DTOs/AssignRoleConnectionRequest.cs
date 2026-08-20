namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class AssignRoleConnectionRequest
{
    public string RoleId { get; set; } = default!;

    public Guid ConnectionId { get; set; }
}