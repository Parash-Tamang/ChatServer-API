using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIChatbot.Api.Middleware;

public class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMappingMiddleware> _logger;

    public ExceptionMappingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMappingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }

        // 🔴 VALIDATION → 400
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Validation failed",
                details = ex.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }

        // 🔴 NOT FOUND → 404
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Resource not found"
            });
        }

        // 🔴 UNAUTHORIZED → 401
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                error = string.IsNullOrWhiteSpace(ex.Message)
                    ? "Unauthorized"
                    : ex.Message
            });
        }

        // 🔴 CONFLICT → 409
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict occurred");

            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "User already Exist or Email Already registered with another user "
            });
        }
       
        // 🔴 MODEL TIMEOUT → 504
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "AI model failed to respond");

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "The Model was unable to respond to the request"
                });
            }
        }

        // 🔴 SYSTEM FAILURE → 500 (custom message)
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "System failure");

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "System was unable to respond to the request"
                });
            }
        }

        // 🔴 FALLBACK → 500
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal server error"
            });
        }
    }
}