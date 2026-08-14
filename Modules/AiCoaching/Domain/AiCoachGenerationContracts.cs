namespace backend.Modules.AiCoaching.Domain;

public sealed record CoachGenerationSource(
    string SourceFingerprint,
    string SnapshotSchemaVersion,
    string SnapshotJson,
    string PromptVersion,
    string ResponseSchemaVersion);

public sealed record CoachGenerationCompletion(
    string Provider,
    string ProviderResponseId,
    string Model,
    int InputTokens,
    int OutputTokens,
    int TotalTokens);
