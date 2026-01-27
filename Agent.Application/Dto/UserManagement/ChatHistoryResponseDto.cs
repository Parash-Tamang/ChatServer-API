using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Application.Dto.UserManagement
{
    public class ChatHistoryResponseDto
    {
        public string SessionId { get; set; }

        public string SessionTitle { get; set; }

        public List<UserMessageResponseDto> Messages { get; set; }
    }

}
