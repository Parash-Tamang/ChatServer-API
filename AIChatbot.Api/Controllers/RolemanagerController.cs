using AIChatbot.Api.Models.RoleManagement;
using AIChatbot.Application.RoleManagement.Commands;
using AIChatbot.Application.RoleManagement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/rolemanager/V1/role-engine")]
[Authorize]
public class RolemanagerController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolemanagerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost("HC2_gen")]
    public async Task<IActionResult> CreateAdmin(CreateAdminRequest req)
    {
        var cmd = new CreateAdminCommand(
            req.FirstName,
            req.LastName,
            req.Email,
            req.Phone,
            req.Password,
            UserId!);

        return Ok(await _mediator.Send(cmd));
    }
    [HttpGet("LC1_listRoles")]
    public async Task<IActionResult> ListRoles()
    {
        return Ok(await _mediator.Send(new ListRolesQuery(UserId!)));
    }

    [HttpPost("LC1_gen")]
    public async Task<IActionResult> CreateRole(CreateRoleRequest req)
    {
        var cmd = new CreateRoleCommand(
            req.RoleName,
            UserId!);

        return Ok(await _mediator.Send(cmd));
    }

    [HttpDelete("Discard_Role/{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        return Ok(await _mediator.Send(
            new DeleteRoleCommand(roleId, UserId!)));
    }

    [HttpGet("LC1_listUser/{roleId}")]
    public async Task<IActionResult> ListUsers(string roleId)
    {
        return Ok(await _mediator.Send(
            new ListUsersByRoleQuery(roleId, UserId!)));
    }

    [HttpDelete("Discard_user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        return Ok(await _mediator.Send(
            new DeleteUserCommand(userId, UserId!)));
    }
}