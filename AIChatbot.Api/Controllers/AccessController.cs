using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/access/V1/Data-Setup-engine")]

// ============================================================
// ONLY SUPER ADMIN
// ============================================================

[Authorize(Policy = "SuperAdminOnly")]

// ============================================================
// RATE LIMITING
// ============================================================

[EnableRateLimiting("RolesPolicy")]

public class AccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccessController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // ============================================================
    // GET DATABASE SCHEMA
    // ============================================================

    [HttpGet("GETschema/{connectionId}")]
    public async Task<IActionResult>
        GetSchema(
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new GetDatabaseSchemaQuery(
                connectionId)));
    }

    // ============================================================
    // ASSIGN DATABASE TO ROLE
    // ============================================================

    [HttpPost("Role-and-DB/assign")]
    public async Task<IActionResult>
        AssignDatabaseToRole(
            [FromBody]
            AssignRoleConnectionRequest request)
    {
        return Ok(await _mediator.Send(
            new AssignRoleConnectionCommand(
                request.RoleId,
                request.ConnectionId)));
    }

    // ============================================================
    // GET DATABASES ASSIGNED TO ROLE
    // ============================================================

    [HttpGet("Role-and-DB/{roleId}")]
    public async Task<IActionResult>
        GetRoleDatabases(
            string roleId)
    {
        return Ok(await _mediator.Send(
            new GetRoleConnectionsQuery(
                roleId)));
    }

    // ============================================================
    // REMOVE DATABASE FROM ROLE
    // ============================================================

    [HttpDelete(
        "Role-and-DB/remove/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        RemoveDatabaseFromRole(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new RemoveRoleConnectionCommand(
                roleId,
                connectionId)));
    }

    // ============================================================
    // SAVE LOOKUP CONFIGURATION
    // ============================================================

    [HttpPost("lookup-for-user-using/save")]
    public async Task<IActionResult>
        SaveLookupConfiguration(
            [FromBody]
            SaveUserLookupConfigurationRequest request)
    {
        return Ok(await _mediator.Send(
            new SaveUserLookupConfigurationCommand(
                request.RoleId,

                request.ConnectionId,

                request.UserTableName,

                request.UserIdColumn,

                request.EmailColumn,

                request.PhoneColumn)));
    }

    // ============================================================
    // GET LOOKUP CONFIGURATION
    // ============================================================

    [HttpGet(
        "lookup-for-user-using/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        GetLookupConfiguration(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new GetUserLookupConfigurationQuery(
                roleId,
                connectionId)));
    }

    // ============================================================
    // DELETE LOOKUP CONFIGURATION
    // ============================================================

    [HttpDelete(
        "lookup-for-user-using/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        DeleteLookupConfiguration(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new DeleteUserLookupConfigurationCommand(
                roleId,
                connectionId)));
    }

    // ============================================================
    // SAVE EXCLUSIONS
    // ============================================================

    [HttpPost("Exclude-from-DB/save")]
    public async Task<IActionResult>
        SaveExclusions(
            [FromBody]
            SaveConnectionExclusionRequest request)
    {
        return Ok(await _mediator.Send(
            new SaveConnectionExclusionCommand(
                request.ConnectionId,
                request.Exclusions)));
    }

    // ============================================================
    // GET EXCLUSIONS
    // ============================================================

    [HttpGet("Exclude-from-DB/{connectionId}")]
    public async Task<IActionResult>
        GetExclusions(
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new GetConnectionExclusionsQuery(
                connectionId)));
    }

    // ============================================================
    // DELETE EXCLUSIONS
    // ============================================================

    [HttpDelete("Exclude-from-DB/{connectionId}")]
    public async Task<IActionResult>
        DeleteExclusions(
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new DeleteConnectionExclusionsCommand(
                connectionId)));
    }

    // ============================================================
    // SAVE RUNTIME PERMISSIONS
    // ============================================================

    [HttpPost("runtime-access/save")]
    public async Task<IActionResult>
     SaveRuntimePermissions(
         [FromBody]
        SaveRuntimePermissionRequest request)
    {
        return Ok(
            await _mediator.Send(
                new SaveRuntimePermissionCommand(
                    request.Role,
                    request.SelectedRoleId,
                    request.Database,
                    request.ConnectionId,
                    request.Permissions)));
    }

    // ============================================================
    // GET RUNTIME PERMISSIONS
    // ============================================================

    [HttpGet(
        "runtime-access/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        GetRuntimePermissions(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new GetRuntimePermissionQuery(
                roleId,
                connectionId)));
    }

    // ============================================================
    // DELETE RUNTIME PERMISSIONS
    // ============================================================

    [HttpDelete(
        "runtime-access/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        DeleteRuntimePermissions(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new DeleteRuntimePermissionCommand(
                roleId,
                connectionId)));
    }

    // ============================================================
    // GET GENERATED VIEW
    // ============================================================

    [HttpGet(
        "runtime-access/view/{roleId}/{connectionId}")]
    public async Task<IActionResult>
        GetRuntimeView(
            string roleId,
            Guid connectionId)
    {
        return Ok(await _mediator.Send(
            new GetRuntimePermissionViewQuery(
                roleId,
                connectionId)));
    }
}