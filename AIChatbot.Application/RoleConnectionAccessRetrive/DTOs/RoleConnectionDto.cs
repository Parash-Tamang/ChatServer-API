using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public sealed class RoleConnectionDto
{
    public string RoleId { get; set; } = default!;

    public string RoleName { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public string ServerName { get; set; } = default!;

    public string DatabaseName { get; set; } = default!;

    public string AuthMode { get; set; } = default!;
}
