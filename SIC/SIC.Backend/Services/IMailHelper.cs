using SIC.Shared.Response;

namespace SIC.Backend.Services;

public interface IMailHelper
{
    Task<ActionResponse<string>> SendMailGmailAsync(string toName, string toEmail, string subject, string body);
}