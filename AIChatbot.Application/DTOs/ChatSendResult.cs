using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.DTOs;

public class ChatSendResult
{
    public Guid ChatSessionId { get; set; }
    public string Reply { get; set; } = default!;
}
