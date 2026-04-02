namespace AIChatbot.web.Dto
{
    public class RoleListDto
    {
        public bool success { get; set; }
        public List<string> roles { get; set; } = ["User"];
    }
}
