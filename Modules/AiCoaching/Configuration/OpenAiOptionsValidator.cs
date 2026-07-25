using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class OpenAiOptionsValidator : IValidateOptions<OpenAiOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenAiOptions options)
    {
        var errors = new List<string>();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps)
            errors.Add("OpenAI:BaseUrl must be an absolute HTTPS URL.");

        if (string.IsNullOrWhiteSpace(options.Model))
            errors.Add("OpenAI:Model is required.");

        if (options.TimeoutSeconds is < 5 or > 120)
            errors.Add("OpenAI:TimeoutSeconds must be between 5 and 120.");

        if (options.MaxOutputTokens is < 128 or > 4096)
            errors.Add("OpenAI:MaxOutputTokens must be between 128 and 4096.");

        if (options.WorkerPollSeconds is < 1 or > 60)
            errors.Add("OpenAI:WorkerPollSeconds must be between 1 and 60.");

        if (options.ProcessingLeaseSeconds is < 60 or > 600)
            errors.Add("OpenAI:ProcessingLeaseSeconds must be between 60 and 600.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
