namespace backend.Modules.AiCoaching.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public bool Enabled { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
    public string Model { get; init; } = "gpt-5-mini";
    public int TimeoutSeconds { get; init; } = 45;
    public int MaxOutputTokens { get; init; } = 1200;
    public int WorkerPollSeconds { get; init; } = 10;
    public int ProcessingLeaseSeconds { get; init; } = 180;
}
