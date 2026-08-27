namespace backend.Modules.User.Domain.Constants;

public static class UserNameRules
{
    public const int MinimumLength = 3;
    public const int MaximumLength = 20;
    public const string Pattern = "^[A-Za-z0-9_]+$";

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();
}
