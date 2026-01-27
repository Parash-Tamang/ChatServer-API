using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Application.Dto.UserManagement
{
    public class ChatRequestDto
    {
        public string? sessionId { get; set; }
        public string message { get; set; } = null!;


    }

}
