using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Startup;

public sealed class AdministrationOptionsValidator : IValidateOptions<AdministrationOptions>
{
    public ValidateOptionsResult Validate(string? name, AdministrationOptions options)
    {
        var errors = options.InitialAdminEmails
            .Where(email => !IsValidEmail(email))
            .Select(email => "Administration:InitialAdminEmails contains an invalid email address.")
            .Distinct()
            .ToList();

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var parsed = new MailAddress(email.Trim());
            return string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
