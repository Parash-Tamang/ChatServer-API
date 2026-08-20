using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class PermissionDictionaryResponse
{
    public object Metadata { get; set; } = default!;

    public Dictionary<string, object>
        Permissions
    { get; set; } = new();
}