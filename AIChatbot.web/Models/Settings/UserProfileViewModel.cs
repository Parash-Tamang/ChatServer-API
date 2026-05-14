namespace AIChatbot.web.Models.Settings
{
    public class UserProfileViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}