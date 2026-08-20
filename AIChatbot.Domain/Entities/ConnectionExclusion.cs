using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Domain.Entities;

public class ConnectionExclusion
{
    public Guid Id { get; set; }

    public Guid ConnectionId { get; set; }

    // dbo
    // dbo.Users
    // dbo.Users.Password
    public string ExclusionPath { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
