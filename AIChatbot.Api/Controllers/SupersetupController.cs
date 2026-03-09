using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/supersetup/V1/setup-engine")]
[Authorize(Roles = "SuperAdmin")]
public class SupersetupController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupersetupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==========================================================
    // 1️⃣ SAVE CONNECTION (Create / Update)
    // ==========================================================
    [HttpPost("connection")]
    public async Task<IActionResult> SaveConnection(
        [FromBody] SaveConnectionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==========================================================
    // 2️⃣ SAVE FUNCTION + SYSTEM PROMPT
    // ==========================================================
    [HttpPost("function")]
    public async Task<IActionResult> SaveFunction(
        [FromBody] SavePromptFunctionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==========================================================
    // 3️⃣ DELETE CONNECTION / FUNCTION
    // ==========================================================
    [HttpDelete]
    public async Task<IActionResult> Delete(
      [FromQuery] Guid? connectionId,
      [FromQuery] Guid? functionId)
    {
        var command = new DeleteSupersetupCommand(connectionId, functionId);

        var result = await _mediator.Send(command);

        return Ok(result);
    }
    // ==========================================================
    // 4️⃣ SAVE GLOBAL PROMPT
    // ==========================================================
    [HttpPost("global")]
    public async Task<IActionResult> SaveGlobalPrompt(
      [FromBody] SaveGlobalPromptCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==========================================================
    // 5️⃣ GET ALL SUPERSETUP DATA
    // ==========================================================
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetSupersetupDataQuery());
        return Ok(result);
    }
}