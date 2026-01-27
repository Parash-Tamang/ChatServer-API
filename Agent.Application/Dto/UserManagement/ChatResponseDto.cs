using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Application.Dto.UserManagement
{
    public class ChatResponseDto
    {
        public string SessionId { get; set; }

        public string SessionTitle { get; set; }
        public UserMessageResponseDto? AssistantMessage { get; set; }
    }
}

