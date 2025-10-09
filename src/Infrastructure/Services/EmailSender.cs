using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public EmailSender(IConfiguration cfg) { _cfg = cfg; }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        var fromName = _cfg["Email:FromName"] ?? "No-Reply";
        var fromAddr = _cfg["Email:FromAddress"] ?? "no-reply@example.com";
        msg.From.Add(new MailboxAddress(fromName, fromAddr));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;

        var body = new BodyBuilder { HtmlBody = htmlBody };
        msg.Body = body.ToMessageBody();

        using var smtp = new SmtpClient();
        var host = _cfg["Email:Smtp:Host"]!;
        var port = int.Parse(_cfg["Email:Smtp:Port"] ?? "587");
        var useStartTls = bool.Parse(_cfg["Email:Smtp:UseStartTls"] ?? "true");
        var user = _cfg["Email:Smtp:Username"];
        var pass = _cfg["Email:Smtp:Password"];

        await smtp.ConnectAsync(host, port, useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        if (!string.IsNullOrEmpty(user))
        {
            await smtp.AuthenticateAsync(user, pass, ct);
        }
        await smtp.SendAsync(msg, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
