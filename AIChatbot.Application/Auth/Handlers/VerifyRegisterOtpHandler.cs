using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Common;
using MediatR;

public class VerifyRegisterOtpHandler
    : IRequestHandler<VerifyRegisterOtpCommand, string>
{
    private readonly IOtpRepository _otpRepo;

    public VerifyRegisterOtpHandler(IOtpRepository otpRepo)
    {
        _otpRepo = otpRepo;
    }

    public async Task<string> Handle(VerifyRegisterOtpCommand request, CancellationToken ct)
    {
        var entry = await _otpRepo.GetLatestByEmailAsync(request.Email);

        if (entry == null)
            throw new UnauthorizedAccessException("OTP not found");

        if (entry.Expiry < DateTime.UtcNow)
        {
            entry.IsUsed = true;
            await _otpRepo.UpdateAsync(entry);
            throw new UnauthorizedAccessException("OTP expired");
        }

        if (!OtpHasher.Verify(request.Otp, entry.OtpHash))
        {
            entry.Attempts++;

            if (entry.Attempts >= 3)
                entry.IsUsed = true;

            await _otpRepo.UpdateAsync(entry);

            throw new UnauthorizedAccessException("Invalid OTP");
        }

        // ✅ OTP used
        entry.IsUsed = true;

        // 🔐 TOKEN
        entry.RegisterToken = Guid.NewGuid().ToString();
        entry.RegisterTokenUsage = 2;
        entry.RegisterTokenExpiry = DateTime.UtcNow.AddMinutes(10);
        entry.IsRegisterVerified = true;

        await _otpRepo.UpdateAsync(entry);

        return entry.RegisterToken!;
    }
}