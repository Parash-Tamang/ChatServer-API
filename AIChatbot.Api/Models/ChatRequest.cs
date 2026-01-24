using System.ComponentModel.DataAnnotations;
using AIChatbot.Domain.Entities;
namespace AIChatbot.Api.Models;

public class ChatRequest
{
    public Guid? ChatSessionId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}
