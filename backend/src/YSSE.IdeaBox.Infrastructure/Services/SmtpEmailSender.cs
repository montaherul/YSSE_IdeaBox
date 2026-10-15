using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// SMTP email delivery configured from the <c>Email</c> configuration section.
/// When email is not enabled/configured it degrades to a logged no-op so local
/// development and CI never depend on a mail server. Message bodies and
/// credentials are never logged.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _options = EmailOptions.From(configuration);
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogInformation(
                "Email delivery is not configured; skipped sending '{Subject}' to {Recipient}.",
                message.Subject, MaskRecipient(message.To));
            return;
        }

        using var mail = new MailMessage
        {
            From = new MailAddress(_options.FromAddress!, _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.TextBody, null, "text/plain"));
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(mail, ct);
    }

    private static string MaskRecipient(string recipient)
    {
        var at = recipient.IndexOf('@');
        if (at <= 1)
        {
            return "***";
        }

        return $"{recipient[0]}***{recipient[at..]}";
    }

    private sealed record EmailOptions(
        bool Enabled, string? Host, int Port, bool UseSsl,
        string? UserName, string? Password, string? FromAddress, string? FromName)
    {
        public bool IsConfigured =>
            Enabled && !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromAddress);

        public static EmailOptions From(IConfiguration configuration)
        {
            var section = configuration.GetSection("Email");
            return new EmailOptions(
                string.Equals(section["Enabled"], "true", StringComparison.OrdinalIgnoreCase),
                section["Host"],
                int.TryParse(section["Port"], out var port) ? port : 587,
                !string.Equals(section["UseSsl"], "false", StringComparison.OrdinalIgnoreCase),
                section["UserName"],
                section["Password"],
                section["FromAddress"],
                section["FromName"] ?? "YSSE Idea Box");
        }
    }
}
