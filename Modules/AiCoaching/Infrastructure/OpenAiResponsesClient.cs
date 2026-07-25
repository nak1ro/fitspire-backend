using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Infrastructure;

public sealed class OpenAiResponsesClient : IGenerativeAiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiResponsesClient> _logger;

    public OpenAiResponsesClient(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiResponsesClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<StructuredAiGenerationResult> GenerateStructuredAsync(
        StructuredAiGenerationRequest request,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var message = CreateRequestMessage(request);
        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiProviderException(AiProviderFailureKind.Timeout,
                "The AI provider did not respond in time.", true);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "OpenAI request failed before a response was received.");
            throw new AiProviderException(AiProviderFailureKind.Network,
                "The AI provider is temporarily unavailable.", true, exception);
        }
    }

    private void EnsureConfigured()
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new AiServiceUnavailableException("AI coaching is not configured.");
    }

    private HttpRequestMessage CreateRequestMessage(StructuredAiGenerationRequest request)
    {
        var body = new OpenAiResponseRequest
        {
            Model = _options.Model,
            Instructions = request.Instructions,
            Input = request.InputJson,
            Store = false,
            SafetyIdentifier = request.SafetyIdentifier,
            MaxOutputTokens = _options.MaxOutputTokens,
            Metadata = new Dictionary<string, string> { ["prompt_version"] = request.PromptVersion },
            Text = new OpenAiTextConfiguration
            {
                Format = new OpenAiJsonSchemaFormat
                {
                    Name = request.SchemaName,
                    Strict = true,
                    Schema = request.OutputSchema
                }
            }
        };

        var message = new HttpRequestMessage(HttpMethod.Post, "responses");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        message.Content = JsonContent.Create(body, options: SerializerOptions);
        return message;
    }

    private async Task<StructuredAiGenerationResult> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            throw CreateHttpFailure(response.StatusCode, response.Headers);

        var envelope = await response.Content.ReadFromJsonAsync<OpenAiResponseEnvelope>(SerializerOptions, cancellationToken);
        if (envelope is null)
            throw new AiProviderException(AiProviderFailureKind.InvalidResponse,
                "The AI provider returned an unreadable response.", true);

        return CreateGenerationResult(envelope);
    }

    private StructuredAiGenerationResult CreateGenerationResult(OpenAiResponseEnvelope envelope)
    {
        if (string.Equals(envelope.Status, "incomplete", StringComparison.OrdinalIgnoreCase))
            throw new AiProviderException(AiProviderFailureKind.IncompleteResponse,
                "The AI provider could not complete the coaching report.", true);

        if (!string.Equals(envelope.Status, "completed", StringComparison.OrdinalIgnoreCase))
            throw new AiProviderException(AiProviderFailureKind.ProviderFailure,
                "The AI provider could not generate a coaching report.", IsTransientStatus(envelope.Status));

        var refusal = envelope.Output?
            .SelectMany(item => item.Content ?? [])
            .FirstOrDefault(content => string.Equals(content.Type, "refusal", StringComparison.OrdinalIgnoreCase));
        if (refusal is not null)
            throw new AiProviderException(AiProviderFailureKind.Refusal,
                "The AI provider could not generate this coaching report.", false);

        var outputJson = envelope.Output?
            .SelectMany(item => item.Content ?? [])
            .FirstOrDefault(content => string.Equals(content.Type, "output_text", StringComparison.OrdinalIgnoreCase))?.Text;
        if (string.IsNullOrWhiteSpace(outputJson) || string.IsNullOrWhiteSpace(envelope.Id) || string.IsNullOrWhiteSpace(envelope.Model))
            throw new AiProviderException(AiProviderFailureKind.InvalidResponse,
                "The AI provider returned an incomplete coaching report.", true);

        var usage = envelope.Usage;
        return new StructuredAiGenerationResult(outputJson, envelope.Id, envelope.Model,
            usage?.InputTokens ?? 0, usage?.OutputTokens ?? 0, usage?.TotalTokens ?? 0);
    }

    private AiProviderException CreateHttpFailure(HttpStatusCode statusCode, HttpResponseHeaders headers)
    {
        var requestId = headers.TryGetValues("x-request-id", out var values) ? values.FirstOrDefault() : null;
        _logger.LogWarning("OpenAI request failed with HTTP status {StatusCode}. RequestId: {RequestId}",
            (int)statusCode, requestId);

        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiProviderException(
                AiProviderFailureKind.Authentication, "AI coaching is temporarily unavailable.", false),
            (HttpStatusCode)429 => new AiProviderException(
                AiProviderFailureKind.RateLimited, "AI coaching is temporarily busy. Please try again shortly.", true,
                retryAfter: GetRetryAfter(headers)),
            _ when (int)statusCode >= 500 => new AiProviderException(
                AiProviderFailureKind.ProviderFailure, "The AI provider is temporarily unavailable.", true),
            _ => new AiProviderException(
                AiProviderFailureKind.ProviderFailure, "The AI provider could not generate a coaching report.", false)
        };
    }

    private static TimeSpan? GetRetryAfter(HttpResponseHeaders headers)
    {
        var retryAfter = headers.RetryAfter;
        if (retryAfter?.Delta is { } delay)
            return delay > TimeSpan.Zero ? delay : null;
        if (retryAfter?.Date is not { } retryAt)
            return null;

        var remaining = retryAt - DateTimeOffset.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : null;
    }

    private static bool IsTransientStatus(string? status) =>
        string.Equals(status, "queued", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "in_progress", StringComparison.OrdinalIgnoreCase);
}
