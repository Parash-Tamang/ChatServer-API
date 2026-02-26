using AIChatbot.Application.DTOs;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.Queries;
using AIChatbot.Application.SuperSetup.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/supersetup/V1/setup-engine")]
[Authorize]
public class SupersetupController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupersetupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ---------------- HC1_conTest ----------------
    [HttpPost("HC1_conTest")]
    public async Task<IActionResult> TestConnection(ConnectionTestDto dto)
    {
        var result = await _mediator.Send(new TestConnectionCommand(dto));

        if (!result)
            return StatusCode(500, "Database connection failed");

        return Ok("Connection successful");
    }

    // ---------------- HC1_integration ----------------
    [HttpPost("HC1_integration")]
    public async Task<IActionResult> Integrate(ConnectionTestDto dto)
    {
        var dbId = await _mediator.Send(
            new IntegrateDatabaseCommand(dto, UserId));

        return Ok(new { databaseId = dbId });
    }

    // ---------------- HC1_getdb ----------------
    [HttpGet("HC1_getdb")]
    public async Task<IActionResult> GetDb()
        => Ok(await _mediator.Send(new GetAllDatabasesQuery(UserId)));

    // ---------------- HC1_getprom ----------------
    [HttpGet("HC1_getprom/{dbId}")]
    public async Task<IActionResult> GetPrompt(Guid dbId)
        => Ok(await _mediator.Send(new GetPromptQuery(dbId, UserId)));

    // ---------------- HC1_setprompt ----------------
    [HttpPost("HC1_setprompt")]
    public async Task<IActionResult> SetPrompt(SetPromptCommand cmd)
    {
        cmd = cmd with { RequestedByUserId = UserId };
        return Ok(await _mediator.Send(cmd));
    }

    // ---------------- HC1_rollback ----------------
    [HttpPost("HC1_rollback/{dbId}")]
    public async Task<IActionResult> Rollback(Guid dbId)
        => Ok(await _mediator.Send(new RollbackPromptCommand(dbId, UserId)));

    [HttpPost("HC1_DBwrite")]
    public async Task<IActionResult> UpdateConnection(UpdateConnectionCommand cmd)
    {
        cmd = cmd with { RequestedByUserId = UserId };
        return Ok(await _mediator.Send(cmd));
    }

    [HttpDelete("HC1_DBErase/{dbId}")]
    public async Task<IActionResult> DeleteConnection(Guid dbId)
    {
        return Ok(await _mediator.Send(
            new DeleteConnectionCommand(dbId, UserId)));
    }
}