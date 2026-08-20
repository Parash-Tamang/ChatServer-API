using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Domain.Entities
{
    public class OtpVerification
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string OtpHash { get; set; } = string.Empty;

        public string ResetToken { get; set; } = string.Empty;
        public DateTime? ResetTokenExpiry { get; set; }
        public string? VerifiedResetToken { get; set; }
        public string? RegisterToken { get; set; }
        public int RegisterTokenUsage { get; set; }
        public DateTime? RegisterTokenExpiry { get; set; }
        public bool IsRegisterVerified { get; set; }
        public bool IsVerified { get; set; }
        public DateTime Expiry { get; set; }

        public int Attempts { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastOtpSentAt { get; set; }
        public int OtpRequestCount { get; set; }
    }
}
