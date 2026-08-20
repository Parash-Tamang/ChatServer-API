using AIChatbot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions
{
    public interface IOtpRepository
    {
        Task SaveAsync(OtpVerification otp);
        Task<OtpVerification?> GetActiveByEmailAsync(string email);
        Task<OtpVerification?> GetLatestByEmailAsync(string email);
        Task InvalidateAsync(string email);
        Task UpdateAsync(OtpVerification otp);
        Task<OtpVerification?> GetByResetTokenAsync(string token);
        Task<OtpVerification?> GetByRegisterTokenAsync(string token);
    }
}
