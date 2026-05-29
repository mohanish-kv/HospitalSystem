using HospitalSystem.API.Domain.Exceptions;
using System.Text.Json;

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
        catch (DomainException ex)
        {
            await WriteError(context, 400, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteError(context, 404, ex.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
        {
            await WriteError(context, 409, ex.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50010)
        {
            await WriteError(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteError(HttpContext ctx, int statusCode, string message)
    {
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = message });
        await ctx.Response.WriteAsync(body);
    }
}
