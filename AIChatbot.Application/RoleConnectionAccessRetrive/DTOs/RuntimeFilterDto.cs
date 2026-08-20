using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs
{
 

    public class RuntimeFilterDto
    {
        [JsonPropertyName("column")]
        public string Column { get; set; } = string.Empty;

        [JsonPropertyName("filter_type")]
        public string FilterType { get; set; } = string.Empty;

        [JsonPropertyName("values")]
        public object? Values { get; set; }
    }
}
