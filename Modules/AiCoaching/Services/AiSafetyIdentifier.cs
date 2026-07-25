using System.Security.Cryptography;
using System.Text;

namespace backend.Modules.AiCoaching.Services;

public static class AiSafetyIdentifier
{
    public static string FromUserId(Guid userId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(userId.ToString("N")));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
