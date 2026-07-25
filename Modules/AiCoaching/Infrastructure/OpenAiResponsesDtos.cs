using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Modules.AiCoaching.Infrastructure;

internal sealed class OpenAiResponseRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("instructions")]
    public required string Instructions { get; init; }

    [JsonPropertyName("input")]
    public required string Input { get; init; }

    [JsonPropertyName("store")]
    public bool Store { get; init; }

    [JsonPropertyName("safety_identifier")]
    public required string SafetyIdentifier { get; init; }

    [JsonPropertyName("max_output_tokens")]
    public int MaxOutputTokens { get; init; }

    [JsonPropertyName("metadata")]
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    [JsonPropertyName("text")]
    public required OpenAiTextConfiguration Text { get; init; }
}

internal sealed class OpenAiTextConfiguration
{
    [JsonPropertyName("format")]
    public required OpenAiJsonSchemaFormat Format { get; init; }

    [JsonPropertyName("verbosity")]
    public string Verbosity { get; init; } = "low";
}

internal sealed class OpenAiJsonSchemaFormat
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "json_schema";

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("strict")]
    public bool Strict { get; init; }

    [JsonPropertyName("schema")]
    public JsonElement Schema { get; init; }
}

internal sealed class OpenAiResponseEnvelope
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("output")]
    public IReadOnlyList<OpenAiOutputItem>? Output { get; init; }

    [JsonPropertyName("error")]
    public OpenAiError? Error { get; init; }

    [JsonPropertyName("incomplete_details")]
    public OpenAiIncompleteDetails? IncompleteDetails { get; init; }

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; init; }
}

internal sealed class OpenAiOutputItem
{
    [JsonPropertyName("content")]
    public IReadOnlyList<OpenAiOutputContent>? Content { get; init; }
}

internal sealed class OpenAiOutputContent
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; init; }
}

internal sealed class OpenAiError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }
}

internal sealed class OpenAiIncompleteDetails
{
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}

internal sealed class OpenAiUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; init; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}
