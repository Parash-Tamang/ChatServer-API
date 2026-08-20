using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;

public class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IAuthService _auth;
    private readonly IOtpRepository _otpRepo;

    public ResetPasswordHandler(IAuthService auth, IOtpRepository otpRepo)
    {
        _auth = auth;
        _otpRepo = otpRepo;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var entry = await _otpRepo.GetByResetTokenAsync(request.ResetToken);

        if (entry == null)
            throw new UnauthorizedAccessException("Invalid or expired token");

        if (!entry.IsVerified)
            throw new UnauthorizedAccessException("OTP not verified");

        if (entry.IsUsed)
            throw new UnauthorizedAccessException("Token already used");

        // ✅ correct expiry check
        if (entry.ResetTokenExpiry == null || entry.ResetTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Reset link expired");

        // 🔐 reset password via Identity token stored earlier
        await _auth.ResetPasswordAsync(
            entry.Email,
            entry.ResetToken,
            request.NewPassword);

        // 🔥 mark used
        entry.IsUsed = true;
        await _otpRepo.UpdateAsync(entry);

        // 🔥 revoke all sessions (VERY IMPORTANT)
        await _auth.RevokeAllAsync(entry.Email);

        return Unit.Value;
    }
}