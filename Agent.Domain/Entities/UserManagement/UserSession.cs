using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Agent.Domain.Entities.UserManagement
{
    public class UserSession
    {
        [Key]
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public required string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? applicationUser { get; set; }

        [Required]
        public required string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property (One-to-Many)
        public ICollection<UserMessage> Messages { get; set; } = new List<UserMessage>();
    }
}
