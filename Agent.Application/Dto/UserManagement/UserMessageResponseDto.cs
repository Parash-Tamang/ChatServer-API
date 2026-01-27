using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Agent.Application.Dto.UserManagement
{
    public class UserMessageResponseDto
    {
        public  string MessageId { get; set; }

        public string SessionId { get; set; }
        public string SenderType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string MessageText { get; set; }
    }
}