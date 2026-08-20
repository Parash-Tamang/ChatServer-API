using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;

        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Add(otp);
            await _context.SaveChangesAsync();
        }
        public async Task<OtpVerification?> GetByResetTokenAsync(string token)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.VerifiedResetToken == token);
        }

        public async Task<OtpVerification?> GetActiveByEmailAsync(string email)
        {
            return await _context.OtpVerifications
                .Where(x => x.Email == email && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
        public async Task<OtpVerification?> GetByRegisterTokenAsync(string token)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.RegisterToken == token &&
                    x.RegisterTokenExpiry > DateTime.UtcNow &&
                    x.RegisterTokenUsage > 0);
        }
        public async Task InvalidateAsync(string email)
        {
            var entries = _context.OtpVerifications
                .Where(x => x.Email == email && !x.IsUsed);

            foreach (var e in entries)
            {
                e.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Update(otp);
            await _context.SaveChangesAsync();
        }
        public async Task<OtpVerification?> GetLatestByEmailAsync(string email)
        {
            return await _context.OtpVerifications
                .Where(x => x.Email == email && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
