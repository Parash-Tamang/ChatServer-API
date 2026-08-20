using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace AIChatbot.Application.Auth.Handlers
{
    public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, bool>
    {
        private readonly IOtpRepository _otpRepo;
        private readonly IEmailService _email;
        private readonly FrontendSettings _frontend;

        public VerifyOtpHandler(
            IOtpRepository otpRepo,
            IEmailService email,
            IOptions<FrontendSettings> frontend)
        {
            _otpRepo = otpRepo;
            _email = email;
            _frontend = frontend.Value;
        }

        public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken ct)
        {
            var entry = await _otpRepo.GetActiveByEmailAsync(request.Email);

            if (entry == null || entry.Expiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("OTP expired");

            if (!OtpHasher.Verify(request.Otp, entry.OtpHash))
                throw new UnauthorizedAccessException("Invalid OTP");
            // 🔐 generate reset token
            var resetToken = Guid.NewGuid().ToString();

            entry.VerifiedResetToken = resetToken;
            entry.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10); // ✅ 10 min expiry
            entry.IsVerified = true;

            await _otpRepo.UpdateAsync(entry);

            // 🔥 build reset link (FIXED)
            var resetLink = $"{_frontend.ResetPasswordUrl}?token={Uri.EscapeDataString(resetToken)}";

            // 🔥 send link via email
            await _email.SendResetLinkAsync(request.Email, resetLink);

            return true;
        }
    }
}