using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Common
{
    public class GenericResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
