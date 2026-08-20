using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Auth.Handlers;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, AuthResult>
{
    private readonly IAuthService _auth;

    private readonly IOtpRepository _otpRepo;

    private readonly IRoleConnectionMappingRepository
        _mappingRepo;

    private readonly IExternalUserLookupService
        _lookupService;

    private readonly IExternalUserMappingRepository
        _externalRepo;

    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _lookupConfigRepo;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly RoleManager<IdentityRole>
        _roleManager;

    public RegisterUserHandler(
        IAuthService auth,

        IOtpRepository otpRepo,

        IRoleConnectionMappingRepository mappingRepo,

        IExternalUserLookupService lookupService,

        IExternalUserMappingRepository externalRepo,

        IRoleConnectionUserLookupConfigurationRepository
            lookupConfigRepo,

        UserManager<ApplicationUser> userManager,

        RoleManager<IdentityRole> roleManager)
    {
        _auth = auth;

        _otpRepo = otpRepo;

        _mappingRepo = mappingRepo;

        _lookupService = lookupService;

        _externalRepo = externalRepo;

        _lookupConfigRepo = lookupConfigRepo;

        _userManager = userManager;

        _roleManager = roleManager;
    }

    public async Task<AuthResult> Handle(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        // ============================================================
        // BASIC VALIDATION
        // ============================================================

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new BadHttpRequestException(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadHttpRequestException(
                "Password is required.");
        }

        // ============================================================
        // OTP TOKEN VALIDATION
        // ============================================================

        var entry =
            await _otpRepo.GetByRegisterTokenAsync(
                request.Token);

        if (entry == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired token.");
        }

        if (!entry.IsRegisterVerified)
        {
            throw new UnauthorizedAccessException(
                "OTP verification pending.");
        }

        if (!string.Equals(
                entry.Email,
                request.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "Token does not belong to this email.");
        }

        // ============================================================
        // ROLE VALIDATION
        // ============================================================

        var role =
            await _roleManager.FindByNameAsync(
                request.Role);

        if (role == null)
        {
            throw new Exception(
                "Selected role does not exist.");
        }

        // ============================================================
        // GET ROLE ↔ DB MAPPING
        // ============================================================

        var roleConnections =
            await _mappingRepo.GetByRoleIdAsync(
                role.Id);

        var roleMapping =
            roleConnections.FirstOrDefault();

        ConnectionString? connection = null;

        RoleConnectionUserLookupConfiguration?
            lookupConfig = null;

        if (roleMapping != null)
        {
            connection = roleMapping.Connection;

            lookupConfig =
                await _lookupConfigRepo.GetAsync(
                    role.Id,
                    connection.Id);
        }

        // ============================================================
        // EXTERNAL DB VALIDATION
        // ============================================================

        ExternalUserLookupResult? externalUser = null;

        if (connection != null)
        {
            externalUser =
                await _lookupService.FindUserAsync(
                    connection,

                    lookupConfig ??
                    new RoleConnectionUserLookupConfiguration
                    {
                        UserTableName = ""
                    },

                    request.Email,
                    request.Phone);

            if (externalUser == null ||
                !externalUser.Found)
            {
                throw new UnauthorizedAccessException(
                    "User details did not match records in assigned database.");
            }
        }

        // ============================================================
        // CONSUME TOKEN
        // ============================================================

        entry.RegisterTokenUsage--;

        await _otpRepo.UpdateAsync(entry);

        // ============================================================
        // CREATE USER
        // ============================================================

        var result =
            await _auth.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.Password,
                request.Role);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                result.Error ??
                "User registration failed.");
        }

        // ============================================================
        // FETCH CREATED USER
        // ============================================================

        var createdUser =
            await _userManager.FindByEmailAsync(
                request.Email);

        if (createdUser == null)
        {
            throw new Exception(
                "Unable to fetch created user.");
        }

        // ============================================================
        // SAVE EXTERNAL USER MAPPING
        // ============================================================

        if (connection != null &&
            externalUser != null)
        {
            await _externalRepo.AddAsync(
                new ExternalUserMapping
                {
                    Id = Guid.NewGuid(),

                    ApplicationUserId =
                        createdUser.Id,

                    ConnectionId =
                        connection.Id,

                    ExternalTable =
                        externalUser.TableName,

                    ExternalUserId =
                        externalUser.UserId,

                    MatchColumn =
                        externalUser.MatchColumn,

                    MatchValue =
                        externalUser.MatchValue,

                    CreatedAt =
                        DateTime.UtcNow
                });
        }

        return result;
    }
}