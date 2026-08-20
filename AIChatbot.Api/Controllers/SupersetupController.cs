using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Application.Supersetup.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/supersetup/V1/setup-engine")]

// ============================================================
// SUPER ADMIN ONLY
// ============================================================

[Authorize(Policy = "SuperAdminOnly")]

// ============================================================
// RATE LIMITING
// ============================================================

[EnableRateLimiting("RolesPolicy")]

public class SupersetupController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupersetupController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==========================================================
    // TEST + SAVE CONNECTION
    // ==========================================================

    [HttpPost("connection/test")]
    public async Task<IActionResult>
        TestConnection(
            [FromBody]
            SaveConnectionCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // CREATE KNOWLEDGEBASE
    // ==========================================================

    [HttpPost("connection/create-kb")]
    public async Task<IActionResult>
        CreateKnowledgebase(
            [FromBody]
            CreateKnowledgebaseCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // ACTIVATE CONNECTION
    // ==========================================================

    [HttpPost("connection/activate")]
    public async Task<IActionResult>
        ActivateConnection(
            [FromBody]
            ActivateConnectionCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // UPDATE KNOWLEDGEBASE
    // ==========================================================

    [HttpPost("connection/update-kb")]
    public async Task<IActionResult>
        UpdateKnowledgebase(
            [FromBody]
            UpdateKnowledgebaseCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // SAVE LOCAL FUNCTION + SYSTEM PROMPT
    // ==========================================================

    [HttpPost("function/Local")]
    public async Task<IActionResult>
        SaveFunction(
            [FromBody]
            SavePromptFunctionCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // DELETE CONNECTION / FUNCTION
    // ==========================================================

    [HttpDelete]
    public async Task<IActionResult>
        Delete(
            [FromQuery]
            Guid? connectionId,

            [FromQuery]
            Guid? functionId)
    {
        var command =
            new DeleteSupersetupCommand(
                connectionId,
                functionId);

        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // SAVE GLOBAL PROMPT
    // ==========================================================

    [HttpPost("Function/global")]
    public async Task<IActionResult>
        SaveGlobalPrompt(
            [FromBody]
            SaveGlobalPromptCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    // ==========================================================
    // SYNC FUNCTION TO GLOBAL
    // ==========================================================

    [HttpPost("functions/sync-to-global")]
    public async Task<IActionResult>
        SyncToGlobal(
            [FromBody]
            SyncToGlobalCommand cmd)
    {
        await _mediator.Send(cmd);

        return Ok(new
        {
            success = true,

            message =
                "Function synced with global"
        });
    }

    // ==========================================================
    // GET ALL CONNECTIONS
    // ==========================================================

    [HttpGet("connections")]
    public async Task<IActionResult>
        GetConnections()
    {
        var result =
            await _mediator.Send(
                new GetConnectionsQuery());

        return Ok(result);
    }

    // ==========================================================
    // GET FUNCTIONS BY CONNECTION
    // ==========================================================

    [HttpGet("connections/{connectionId}/functions")]
    public async Task<IActionResult>
        GetFunctionsByConnection(
            Guid connectionId)
    {
        var result =
            await _mediator.Send(
                new GetFunctionsByConnectionQuery(
                    connectionId));

        return Ok(result);
    }

    // ==========================================================
    // GET FUNCTION BY ID
    // ==========================================================

    [HttpGet("functions/{functionId}")]
    public async Task<IActionResult>
        GetFunctionById(
            Guid functionId)
    {
        var result =
            await _mediator.Send(
                new GetFunctionByIdQuery(
                    functionId));

        return Ok(result);
    }

    // ==========================================================
    // GET GLOBAL FUNCTION NAMES
    // ==========================================================

    [HttpGet("functions/global/Global_function")]
    public async Task<IActionResult>
        GetGlobalFunctionNames()
    {
        var result =
            await _mediator.Send(
                new GetGlobalFunctionNamesQuery());

        return Ok(result);
    }
}