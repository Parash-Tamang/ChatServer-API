using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        // =============================
        // 🔴 PRE-CHECK: CONTENT TYPE
        // =============================
        if (context.Request.ContentType != null &&
            !context.Request.ContentType.Contains("application/json") &&
            context.Request.Method != HttpMethods.Get)
        {
            _logger.LogWarning("Unsupported media type: {ContentType}", context.Request.ContentType);

            await WriteSafeResponse(
                context,
                StatusCodes.Status415UnsupportedMediaType,
                "Unsupported media type. Only 'application/json' is allowed.");

            return;
        }

        try
        {
            await _next(context);
        }

        // =============================
        // 🔴 422 → VALIDATION (FluentValidation)
        // =============================
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");

            await WriteSafeResponse(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                ex.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                }));
        }

        // =============================
        // 🔴 400 → BAD REQUEST
        // =============================
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "Bad request");

            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? "Invalid request"
                : ex.Message;

            await WriteSafeResponse(
                context,
                ex.StatusCode != 0 ? ex.StatusCode : StatusCodes.Status400BadRequest,
                message);
        }

        // =============================
        // 🔴 404 → NOT FOUND
        // =============================
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");

            await WriteSafeResponse(
                context,
                StatusCodes.Status404NotFound,
                ex.Message ?? "Resource not found");
        }

        // =============================
        // 🔴 401 → UNAUTHORIZED
        // =============================
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");

            await WriteSafeResponse(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized access");
        }

        // =============================
        // 🔴 409 → CONFLICT
        // =============================
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict");

            await WriteSafeResponse(
                context,
                StatusCodes.Status409Conflict,
                ex.Message ?? "Conflict occurred");
        }

        // =============================
        // 🔴 504 → TIMEOUT
        // =============================
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout");

            await WriteSafeResponse(
                context,
                StatusCodes.Status504GatewayTimeout,
                "Request timed out");
        }

        // =============================
        // 🔴 500 → APPLICATION ERROR
        // =============================
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Application error");

            await WriteSafeResponse(
                context,
                StatusCodes.Status500InternalServerError,
                "Application error occurred");
        }

        // =============================
        // 🔴 500 → FALLBACK
        // =============================
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await WriteSafeResponse(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error");
        }
    }

    // =============================
    // ✅ MASTER RESPONSE FORMAT
    // =============================
    private static async Task WriteSafeResponse(
        HttpContext context,
        int statusCode,
        string message,
        object? details = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            status = statusCode,
            message,
            details // null if not provided
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}