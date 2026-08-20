using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Domain.Entities;

public class LocalFunctionBlock
{
    public Guid Id { get; set; }

    public Guid ConnectionId { get; set; }

    public string FunctionName { get; set; } = string.Empty;

    public Guid GlobalFunctionId { get; set; }

    public bool IsOverride { get; set; }

    public string? OverridePrompt { get; set; }

    public int? OverrideVersion { get; set; }

    public DateTime CreatedAt { get; set; }
}
