using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Application.Dto.UserManagement
{
    public class UserMessageRequestDto
    {
        public string SessionId { get; set; }
        public string MessageText { get; set; }
        public string SenderType { get; set; }
    }
}

