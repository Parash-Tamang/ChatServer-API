namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class UserLookupConfigurationDto
{
    public string UserTableName { get; set; } = default!;

    public string UserIdColumn { get; set; } = default!;

    public string? EmailColumn { get; set; }

    public string? PhoneColumn { get; set; }
}
