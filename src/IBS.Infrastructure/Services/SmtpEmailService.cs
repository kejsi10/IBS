using IBS.BuildingBlocks.Application;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IBS.Infrastructure.Services;

/// <summary>
/// Production email service that sends emails via SMTP using MailKit.
/// </summary>
public sealed class SmtpEmailService(
    IOptions<EmailSettings> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_settings.FromName ?? "IBS", _settings.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));

        if (message.Cc is { Count: > 0 })
        {
            foreach (var cc in message.Cc)
                mimeMessage.Cc.Add(MailboxAddress.Parse(cc));
        }

        if (message.Bcc is { Count: > 0 })
        {
            foreach (var bcc in message.Bcc)
                mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc));
        }

        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new TextPart("html") { Text = message.HtmlBody };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}

/// <summary>
/// Configuration settings for SMTP email delivery.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets whether to use SSL.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the from email address.
    /// </summary>
    public string FromAddress { get; set; } = "noreply@ibs.local";

    /// <summary>
    /// Gets or sets the from display name.
    /// </summary>
    public string? FromName { get; set; } = "IBS";

    /// <summary>
    /// Gets or sets whether to use the console logger instead of SMTP.
    /// </summary>
    public bool UseConsole { get; set; }
}
