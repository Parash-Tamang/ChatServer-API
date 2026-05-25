namespace AIChatbot.web.Dto
{
    public class AssignRoleConnectionDto
    {
        public string RoleId { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }
    }
}