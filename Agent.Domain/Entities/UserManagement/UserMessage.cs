using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Domain.Entities.UserManagement
{
    public class UserMessage
    {
        [Key]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(4024)]
        public required string MessageText { get; set; }

        [Required]
        [StringLength(10)]
        public required string SenderType { get; set; } = "user"; // ai or user

        [Required]
        public required string SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public UserSession? userSession { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
