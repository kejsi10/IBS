using IBS.BuildingBlocks.Application;
using Microsoft.Extensions.Logging;

namespace IBS.Infrastructure.Services;

/// <summary>
/// Development email service that logs emails to the console instead of sending them.
/// </summary>
public sealed class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    /// <inheritdoc />
    public Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            """
            ========== EMAIL ==========
            To:      {To}
            Subject: {Subject}
            CC:      {Cc}
            BCC:     {Bcc}
            Body:
            {Body}
            ===========================
            """,
            message.To,
            message.Subject,
            message.Cc != null ? string.Join(", ", message.Cc) : "(none)",
            message.Bcc != null ? string.Join(", ", message.Bcc) : "(none)",
            message.HtmlBody);

        return Task.CompletedTask;
    }
}
