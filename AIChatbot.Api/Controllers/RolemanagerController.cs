using System.Security.Claims;

using AIChatbot.Api.Models.RoleManagement;
using AIChatbot.Application.RoleManagement.Commands;
using AIChatbot.Application.RoleManagement.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/rolemanager/V1/role-engine")]

// ============================================================
// DEFAULT: SUPER ADMIN ONLY
// ============================================================

[Authorize(Policy = "SuperAdminOnly")]

// ============================================================
// RATE LIMITING
// ============================================================

[EnableRateLimiting("RolesPolicy")]

public class RolemanagerController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolemanagerController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // ============================================================
    // RESOLVE USER ID
    // ============================================================

    private string? UserId =>
        User.FindFirstValue(
            ClaimTypes.NameIdentifier)
        ??
        User.FindFirstValue("sub")
        ??
        User.FindFirstValue("userId");

    // ============================================================
    // LIST ROLES
    // ============================================================

    [HttpGet("LC1_listRoles")]
    public async Task<IActionResult>
        ListRoles()
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new ListRolesQuery(
                UserId)));
    }

    // ============================================================
    // CREATE ROLE
    // ============================================================

    [HttpPost("LC1_gen")]
    public async Task<IActionResult>
        CreateRole(
            [FromBody]
            CreateRoleRequest req)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        var cmd =
            new CreateRoleCommand(
                req.RoleName,
                UserId);

        return Ok(await _mediator.Send(cmd));
    }

    // ============================================================
    // DELETE ROLE
    // ============================================================

    [HttpDelete("Discard_Role/{roleId}")]
    public async Task<IActionResult>
        DeleteRole(
            string roleId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new DeleteRoleCommand(
                roleId,
                UserId)));
    }

    // ============================================================
    // LIST USERS BY ROLE
    // ============================================================

    [HttpGet("LC1_listUser/{roleId}")]
    public async Task<IActionResult>
        ListUsers(
            string roleId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new ListUsersByRoleQuery(
                roleId,
                UserId)));
    }


    [HttpGet(
    "roles-by-connection/{connectionId:guid}")]
    public async Task<IActionResult>
    GetRolesByConnection(
        Guid connectionId)
    {
        var result =
            await _mediator.Send(
                new GetRolesByConnectionQuery(
                    connectionId));

        return Ok(result);
    }

    // ============================================================
    // DELETE USER
    // ============================================================
    // ACCESSIBLE BY ANY AUTHENTICATED USER
    // ============================================================

    [Authorize]

    [HttpDelete("Discard_user/{userId}")]
    public async Task<IActionResult>
        DeleteUser(
            string userId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new DeleteUserCommand(
                userId,
                UserId)));
    }
}