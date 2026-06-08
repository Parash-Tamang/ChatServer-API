namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Model for Chat/Index.cshtml page
    /// Passed from ChatController to View
    /// </summary>
    public class ChatPageModel
    {
        // User info
        public string UserName { get; set; } = "User";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<string> Roles { get; set; } = new();

        // All chat sessions for sidebar
        public List<ChatSessionDto> Sessions { get; set; } = new();

        // Current selected session
        public Guid? CurrentSessionId { get; set; }

        // Messages in current session
        public List<ChatMessageDto> CurrentSessionMessages { get; set; } = new();
    }
}
