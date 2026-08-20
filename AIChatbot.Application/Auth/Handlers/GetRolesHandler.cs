using AIChatbot.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class GetRolesHandler : IRequestHandler<GetRolesQuery, List<string>>
{
    private readonly IOtpRepository _otpRepo;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GetRolesHandler(
        IOtpRepository otpRepo,
        RoleManager<IdentityRole> roleManager)
    {
        _otpRepo = otpRepo;
        _roleManager = roleManager;
    }

    public async Task<List<string>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var entry = await _otpRepo.GetByRegisterTokenAsync(request.Token);

        if (entry == null ||
            !entry.IsRegisterVerified ||
            entry.RegisterTokenExpiry < DateTime.UtcNow ||
            entry.RegisterTokenUsage <= 0)
        {
            throw new UnauthorizedAccessException("Invalid or expired token");
        }

        // 🔥 decrease usage
        entry.RegisterTokenUsage--;
        await _otpRepo.UpdateAsync(entry);

        var roles = _roleManager.Roles
            .Where(r => r.Name != "SuperAdmin")
            .Select(r => r.Name!)
            .ToList();

        return roles;
    }
}