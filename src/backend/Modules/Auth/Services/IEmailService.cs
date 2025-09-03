namespace backend.Modules.Auth.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent);
    Task SendMockEmailAsync(string to, string subject, string htmlContent);
}