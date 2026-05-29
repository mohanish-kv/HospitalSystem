using System.Text.Json;
using HospitalSystem.API.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace HospitalSystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppointmentConflictException ex)
        {
            _logger.LogWarning(ex, "A conflicting appointment request was rejected.");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            _logger.LogWarning(ex, "A duplicate resource was rejected.");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { error = "A resource with the same unique value already exists." });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation error");
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Requested resource was not found");
            await WriteError(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
        {
            _logger.LogWarning(ex, "Duplicate patient contact information");
            await WriteError(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 50010)
        {
            _logger.LogWarning(ex, "Doctor unavailable for requested appointment");
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteError(HttpContext ctx, int statusCode, string message)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        await ctx.Response.WriteAsync(body);
    }
}
