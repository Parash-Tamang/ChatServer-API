using System;

namespace AIChatbot.Application.Common
{
    public class LlmResponse
    {
        public bool Success { get; set; }

        public string? Query { get; set; }

        public string? Message { get; set; }

        public string? SqlGenerated { get; set; }
        public bool ExcelGenerated { get; set; }
        public string? ExcelJson { get; set; }
        public object? Columns { get; set; }

        public object? Rows { get; set; }

        public int RowCount { get; set; }

        public bool WasReconstructed { get; set; }

        public bool ClarificationNeeded { get; set; }

        public object? TokenUsage { get; set; }

        // =====================================================
        // SESSION CONTEXT
        // =====================================================

        public string? RefinedQuery { get; set; }

        public string? IntentDetected { get; set; }

        public object? TablesUsed { get; set; }

        public object? Filters { get; set; }

        public object? SessionContext { get; set; }

        // =====================================================
        // GRAPH
        // =====================================================

        public string? GraphType { get; set; }

        public string? GraphTitle { get; set; }

        public string? GraphReasoning { get; set; }

        public string? GraphImageUrl { get; set; }

        public string? GraphImagePath { get; set; }
        public string? GraphImageBase64 { get; set; }

        // =====================================================
        // CONNECTION
        // =====================================================

        public Guid? ConnectionStringId { get; set; }

        // =====================================================
        // RAW RESPONSE
        // =====================================================

        public string? FullResponseJson { get; set; }
    }
}