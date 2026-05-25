namespace AIChatbot.web.Dto
{
    public class SchemaItemDto
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
    }

    public class ExclusionSaveDto
    {
        public Guid ConnectionId { get; set; }
        public List<ExclusionSchemaDto> Exclusions { get; set; } = new();
    }

    public class ExclusionSchemaDto
    {
        public string SchemaName { get; set; } = string.Empty;
        public List<ExclusionTableDto> Tables { get; set; } = new();
    }

    public class ExclusionTableDto
    {
        public string TableName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
    }
}