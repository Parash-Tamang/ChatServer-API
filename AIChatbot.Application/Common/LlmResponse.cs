using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Common
{
    public class LlmResponse
    {
        public bool Success { get; set; }
        public string? Query { get; set; }
        public string? Message { get; set; }
        public string? SqlGenerated { get; set; }

        public object? Columns { get; set; }
        public object? Rows { get; set; }

        public int RowCount { get; set; }
        public bool WasReconstructed { get; set; }
        public bool ClarificationNeeded { get; set; }

        public object? TokenUsage { get; set; }
    }

}
