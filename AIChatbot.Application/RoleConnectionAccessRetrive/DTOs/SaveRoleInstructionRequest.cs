namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class SaveRoleInstructionRequest
{
    public string RoleId { get; set; } = default!;

    public string InstructionSet { get; set; } = default!;
}