using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class SendRegisterOtpHandler
    : IRequestHandler<SendRegisterOtpCommand, bool>
{
    private readonly IOtpRepository _otpRepo;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;

    public SendRegisterOtpHandler(
    IOtpRepository otpRepo,
    IEmailService email,
    UserManager<ApplicationUser> userManager)
    {
        _otpRepo = otpRepo;
        _email = email;
        _userManager = userManager;
    }

    public async Task<bool> Handle(SendRegisterOtpCommand request, CancellationToken ct)
    {
        var entry = await _otpRepo.GetLatestByEmailAsync(request.Email);

        // 🔥 RATE LIMIT: max 3 per 3 minutes
        if (entry != null &&
            entry.LastOtpSentAt.HasValue &&
            entry.LastOtpSentAt > DateTime.UtcNow.AddMinutes(-3) &&
            entry.OtpRequestCount >= 3)
        {
            throw new Exception("Too many requests. Try again later.");
        }

        // 🔥 COOLDOWN: 60 seconds
        if (entry != null &&
            entry.LastOtpSentAt.HasValue &&
            entry.LastOtpSentAt > DateTime.UtcNow.AddSeconds(-60))
        {
            throw new Exception("Please wait before requesting another OTP.");
        }

        var otp = new Random().Next(100000, 999999).ToString();
        var hash = OtpHasher.Hash(otp);

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
            OtpRequestCount = (entry?.OtpRequestCount ?? 0) + 1
        };

        await _otpRepo.SaveAsync(newEntry);

        await _email.SendOtpAsync(request.Email, otp);

        return true;
    }
}