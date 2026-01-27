using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Agent.Domain.Entities.UserManagement
{
   
        public class RefreshToken
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public string UserId { get; set; }
            [ForeignKey(nameof(UserId))]
            public ApplicationUser User { get; set; }

            public string Token { get; set; }

            public bool IsUsed { get; set; }
            public bool IsRevoked { get; set; }

            public DateTime AddedDate { get; set; } = DateTime.Now;
            public DateTime ExpiryDate { get; set; }
        }

}
