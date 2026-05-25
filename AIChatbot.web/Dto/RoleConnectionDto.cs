namespace AIChatbot.web.Dto
{
    public class RoleConnectionDto
    {
        public Guid Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public RoleConnectionDetailDto? Connection { get; set; }
    }

    public class RoleConnectionDetailDto
    {
        public Guid Id { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string? AuthMode { get; set; }
        public bool IsActive { get; set; }
        public bool Verified { get; set; }
    }
}