using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class PermissionFilterDto
{
    public string Column { get; set; } = string.Empty;

    public string FilterType { get; set; } = string.Empty;

    public object? Values { get; set; }
}