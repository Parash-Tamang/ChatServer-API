namespace AIChatbot.Api.Models.Agent;

public class GetDbFunctionByIdRequest
{
    public Guid ConnectionId { get; set; }

    public Guid FunctionId { get; set; }
}