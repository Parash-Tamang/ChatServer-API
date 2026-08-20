using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class SaveConnectionExclusionRequest
{
    public Guid ConnectionId { get; set; }

    public List<ConnectionExclusionDto> Exclusions { get; set; } = [];
}
