namespace WebApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendEmailAsync(List<string> toAddresses, string subject, string body, bool isHtml = true);
    }
}
