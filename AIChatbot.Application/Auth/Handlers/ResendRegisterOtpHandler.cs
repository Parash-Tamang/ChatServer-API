using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Common;
using MediatR;
using AIChatbot.Domain.Entities;

public class ResendRegisterOtpHandler
    : IRequestHandler<ResendRegisterOtpCommand, bool>
{
    private readonly IOtpRepository _otpRepo;
    private readonly IEmailService _email;

    public ResendRegisterOtpHandler(
        IOtpRepository otpRepo,
        IEmailService email)
    {
        _otpRepo = otpRepo;
        _email = email;
    }

    public async Task<bool> Handle(ResendRegisterOtpCommand request, CancellationToken ct)
    {
        var entry = await _otpRepo.GetLatestByEmailAsync(request.Email);

        if (entry == null)
            throw new Exception("No OTP request found. Please start again.");

        // 🔥 COOLDOWN (60 sec)
        if (entry.LastOtpSentAt.HasValue &&
            entry.LastOtpSentAt > DateTime.UtcNow.AddSeconds(-60))
        {
            throw new Exception("Please wait before requesting another OTP.");
        }

        // 🔥 RATE LIMIT (max 3 in 3 mins)
        if (entry.LastOtpSentAt.HasValue &&
            entry.LastOtpSentAt > DateTime.UtcNow.AddMinutes(-3) &&
            entry.OtpRequestCount >= 3)
        {
            throw new Exception("Too many requests. Try again later.");
        }

        // 🔐 generate new OTP
        var otp = new Random().Next(100000, 999999).ToString();
        var hash = OtpHasher.Hash(otp);

        // ❗ invalidate old OTP
        await _otpRepo.InvalidateAsync(request.Email);

        var newEntry = new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            OtpHash = hash,
            Expiry = DateTime.UtcNow.AddMinutes(3),
            Attempts = 0,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            LastOtpSentAt = DateTime.UtcNow,
            OtpRequestCount = entry.OtpRequestCount + 1
        };

        await _otpRepo.SaveAsync(newEntry);

        await _email.SendOtpAsync(request.Email, otp);

        return true;
    }
}