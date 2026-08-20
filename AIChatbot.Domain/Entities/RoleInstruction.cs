namespace AIChatbot.Domain.Entities;

public class RoleInstruction
{
    public Guid Id { get; set; }

    public string RoleId { get; set; } = default!;

    public string InstructionSet { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}