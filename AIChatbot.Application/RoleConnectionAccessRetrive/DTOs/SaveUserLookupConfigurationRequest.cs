using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class SaveUserLookupConfigurationRequest
{
    public string RoleId { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public string UserTableName { get; set; } = default!;

    public string UserIdColumn { get; set; } = default!;

    public string? EmailColumn { get; set; }

    public string? PhoneColumn { get; set; }
}
