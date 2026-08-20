using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using AIChatbot.Domain.Entities;
using MediatR;
using AIChatbot.Application.Common;

public class ForgotPasswordHandler
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly IAuthService _auth;
    private readonly IOtpRepository _otpRepo;
    private readonly IEmailService? _email;

    public ForgotPasswordHandler(
        IAuthService auth,
        IOtpRepository otpRepo,
        IEmailService? email)
    {
        _auth = auth;
        _otpRepo = otpRepo;
        _email = email;
    }

    public async Task<ForgotPasswordResult> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 🔍 Validate email exists (optional but recommended)
            var token = await _auth.GeneratePasswordResetTokenAsync(request.Email);

            if (string.IsNullOrEmpty(token))
            {
                return new ForgotPasswordResult
                {
                    Success = false,
                    Message = "User not found or unable to generate reset token."
                };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var hash = OtpHasher.Hash(otp);

            await _otpRepo.InvalidateAsync(request.Email);

            var entry = new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                OtpHash = hash,
                ResetToken = token,
                Expiry = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                IsUsed = false,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpRepo.SaveAsync(entry);

            if (_email != null)
            {
                await _email.SendOtpAsync(request.Email, otp);
            }

            return new ForgotPasswordResult
            {
                Success = true,
                Message = "OTP sent successfully."
            };
        }
        catch (Exception)
        {
            // ❌ Don't expose internal errors
            return new ForgotPasswordResult
            {
                Success = false,
                Message = "Something went wrong. Please try again."
            };
        }
    }
}