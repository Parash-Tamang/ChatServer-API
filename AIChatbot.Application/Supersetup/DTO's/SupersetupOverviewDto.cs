using AIChatbot.Application.Supersetup.DTOs;
namespace AIChatbot.Application.Supersetup.DTOs
{
    public class SupersetupOverviewDto
    {


        public List<ConnectionDto> Connections { get; set; } = new();
        public Dictionary<Guid, List<FunctionDto>> Functions { get; set; } = new();
        public List<FunctionDto> GlobalPrompt { get; set; } = new();
    }
}