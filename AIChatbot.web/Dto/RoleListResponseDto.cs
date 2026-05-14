namespace AIChatbot.web.Dto
{
    public class RoleListResponseDto
    {
        public bool Success { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
