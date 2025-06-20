using Resend;

namespace backend.Modules.Auth.Services;

public class ResendEmailService : IEmailService
{
    private readonly ResendClient _client;
    private readonly string _senderEmail;

    public ResendEmailService(ResendClient client, IConfiguration config)
    {
        _client = client;
        _senderEmail = config["Resend:SenderEmail"];
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        var message = new EmailMessage
        {
            From = _senderEmail,
            To = new[] { to },
            Subject = subject,
            HtmlBody = htmlContent
        };

        await _client.EmailSendAsync(message);
    }

    public Task SendMockEmailAsync(string to, string subject, string htmlContent)
    {
        Console.WriteLine("[MOCK EMAIL]");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Content: {htmlContent}");
        return Task.CompletedTask;
    }
}