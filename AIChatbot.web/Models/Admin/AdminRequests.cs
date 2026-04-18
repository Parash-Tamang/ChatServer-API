namespace AIChatbot.web.Models.Admin
{
    public class DeleteFunctionRequest
    {
        public Guid ConnectionId { get; set; }

        public string FunctionId { get; set; }
    }

    public class SetPromptingModeRequest
    {
        public string ConnectionId { get; set; }
        public int PromptingMode { get; set; }
    }
}