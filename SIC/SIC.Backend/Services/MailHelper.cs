using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SIC.Shared.Response;

namespace SIC.Backend.Services;

public class MailHelper : IMailHelper
{
    private readonly IConfiguration _configuration;

    public MailHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ActionResponse<string>> SendMailGmailAsync(string toName, string toEmail, string subject, string body)
    {
        try
        {
            var from = _configuration["Mail:From"];
            var name = _configuration["Mail:NameEs"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(password))
            {
                return new ActionResponse<string>
                {
                    Success = false,
                    Message = "La configuración de correo (Mail:From / Mail:Password) no está definida."
                };
            }

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(name, from));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Gmail SMTP: puerto 587 + STARTTLS
            await client.ConnectAsync(
                smtp,
                int.Parse(port!),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            // Debe ser la cuenta Gmail + App Password
            await client.AuthenticateAsync(from, password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);

            return new ActionResponse<string>
            {
                Success = true,
                Result = toEmail
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<string>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}