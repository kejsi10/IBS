namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Abstraction for sending emails from the application.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
