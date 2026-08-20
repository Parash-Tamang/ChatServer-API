using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs
{ 


public class RuntimeTablePermissionDto
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("access_level")]
    public string AccessLevel { get; set; } = string.Empty;

    [JsonPropertyName("required_filters")]
    public List<RuntimeFilterDto>
        RequiredFilters
    { get; set; } = new();
} };