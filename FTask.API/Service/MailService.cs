using Duende.IdentityServer.Extensions;
using MailKit.Net.Smtp;
using MimeKit;

namespace FTask.API.Service;

public class EmailConfig
{
    public string? From { get; set; }
    public string? SmtpServer { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class Message
{
    public string? To { get; set; }
    public IEnumerable<string>? CC { get; set; }
    public string Subject { get; set; } = "";
    public string Content { get; set; } = "";
    public IFormFileCollection? Attachments { get; set; }
}

public interface IMailService
{
    Task SendMailAsync(Message message);
}

internal class MailService : IMailService
{
    private readonly EmailConfig _emailConfig;

    public MailService(EmailConfig emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public async Task SendMailAsync(Message message)
    {
        var emailMesssage = CreateEmailMessage(message);
        await SendAsync(emailMesssage);
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
        emailMessage.To.Add(new MailboxAddress("email", message.To));
        IEnumerable<MailboxAddress>? CC = message.CC?.Select(e => new MailboxAddress("email", e));
        if (!CC.IsNullOrEmpty())
        {
            emailMessage.Cc.AddRange(CC);
        }
        emailMessage.Subject = message.Subject;

        // Build body content of email message
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.Content
        };
        if (message.Attachments != null && message.Attachments.Any())
        {
            byte[] fileBytes;
            foreach (var attachment in message.Attachments)
            {
                using (var ms = new MemoryStream())
                {
                    attachment.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
            }
        }

        emailMessage.Body = bodyBuilder.ToMessageBody();
        return emailMessage;
    }

    private async Task SendAsync(MimeMessage mailMessage)
    {
        using (var client = new SmtpClient())
        {
            client.CheckCertificateRevocation = false;
            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                await client.SendAsync(mailMessage);
            }
            catch
            {
                //log an error message or throw an exception or both
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
