using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Common
{
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
        public bool UseConsole { get; set; }
    }
}
