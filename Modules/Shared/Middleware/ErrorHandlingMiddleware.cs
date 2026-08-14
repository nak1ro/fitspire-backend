using System.Net;
using System.Text.Json;
using backend.Modules.Shared.Domain;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Shared.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        var response = new
        {
            status = (int)statusCode,
            title = GetTitle(exception),
            detail = exception.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        if (exception is CoachQuestionQuotaExceededException quota)
        {
            var retryAfter = Math.Max(1, (int)Math.Ceiling((quota.ResetAtUtc - DateTime.UtcNow).TotalSeconds));
            context.Response.Headers.RetryAfter = retryAfter.ToString();
        }

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
        CoachQuestionQuotaExceededException => HttpStatusCode.TooManyRequests,
        ConflictException => HttpStatusCode.Conflict,
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => HttpStatusCode.Conflict,
        NotFoundException => HttpStatusCode.NotFound,
        System.Security.Authentication.AuthenticationException => HttpStatusCode.Unauthorized,
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
        CoachQuestionQuotaExceededException => "Coach question limit reached",
        ConflictException => "Conflict",
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => "Conflict",
        NotFoundException => "Resource not found",
        System.Security.Authentication.AuthenticationException => "Unauthorized",
        UnauthorizedAccessException => "Forbidden",
        _ => "Server error"
    };
}
