namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class RuntimePermissionViewDto
{
    public string ViewName { get; set; } = default!;

    public string ViewSql { get; set; } = default!;

    public UserLookupConfigurationDto? UserLookupConfiguration { get; set; }
}