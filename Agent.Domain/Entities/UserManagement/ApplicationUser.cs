using System;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // v8.0.20

namespace Agent.Domain.Entities.UserManagement
{
    public class ApplicationUser : IdentityUser
    {

        // Navigation property (One-to-Many)
        public required string  FullName { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }
}
