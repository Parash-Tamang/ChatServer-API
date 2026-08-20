using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class TableExclusionDto
{
    public string TableName { get; set; } = default!;

    public List<string> Columns { get; set; } = [];
}
