using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class PermissionTableDto
{
    public string Reason { get; set; } = string.Empty;

    public string AccessLevel { get; set; } = string.Empty;

    public List<PermissionFilterDto>
        RequiredFilters
    { get; set; } = new();
}