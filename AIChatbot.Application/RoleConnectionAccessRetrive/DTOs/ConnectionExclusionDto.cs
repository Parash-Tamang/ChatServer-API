using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class ConnectionExclusionDto
{
    public string SchemaName { get; set; } = default!;

    public List<TableExclusionDto> Tables { get; set; } = [];
}
