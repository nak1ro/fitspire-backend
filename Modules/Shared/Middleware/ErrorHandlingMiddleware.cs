using System.Net;
using System.Text.Json;
using backend.Modules.Shared.Domain;
using backend.Modules.AiCoaching.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Shared.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var response = new
        {
            status = (int)statusCode,
            title = GetTitle(exception),
            detail = exception.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => HttpStatusCode.BadRequest,
        DomainException => HttpStatusCode.BadRequest,
        InvalidOperationException => HttpStatusCode.BadRequest,
        StorageUnavailableException => HttpStatusCode.ServiceUnavailable,
        AiServiceUnavailableException => HttpStatusCode.ServiceUnavailable,
        AiProviderException { IsRetryable: true } => HttpStatusCode.ServiceUnavailable,
        AiProviderException => HttpStatusCode.ServiceUnavailable,
        ConflictException => HttpStatusCode.Conflict,
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => HttpStatusCode.Conflict,
        NotFoundException => HttpStatusCode.NotFound,
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        _ => HttpStatusCode.InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        ValidationException => "Validation failed",
        DomainException => "Domain rule violation",
        InvalidOperationException => "Invalid operation",
        StorageUnavailableException => "Storage service unavailable",
        AiServiceUnavailableException => "AI coaching unavailable",
        AiProviderException { IsRetryable: true } => "AI coaching temporarily unavailable",
        AiProviderException => "AI coaching unavailable",
        ConflictException => "Conflict",
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => "Conflict",
        NotFoundException => "Resource not found",
        UnauthorizedAccessException => "Forbidden",
        _ => "Server error"
    };
}
