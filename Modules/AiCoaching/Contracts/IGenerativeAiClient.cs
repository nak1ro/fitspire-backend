using System.Text.Json;

namespace backend.Modules.AiCoaching.Contracts;

public interface IGenerativeAiClient
{
    Task<StructuredAiGenerationResult> GenerateStructuredAsync(
        StructuredAiGenerationRequest request,
        CancellationToken cancellationToken);
}

public sealed record StructuredAiGenerationRequest(
    string Instructions,
    string InputJson,
    string SchemaName,
    JsonElement OutputSchema,
    string SafetyIdentifier,
    string PromptVersion);

public sealed record StructuredAiGenerationResult(
    string OutputJson,
    string ProviderResponseId,
    string Model,
    int InputTokens,
    int OutputTokens,
    int TotalTokens);
