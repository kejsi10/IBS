using IBS.BuildingBlocks.Application;
using IBS.Claims.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IBS.Claims.Application.EventHandlers;

/// <summary>
/// Sends email notification when a claim is approved.
/// </summary>
public sealed class ClaimApprovedNotificationHandler(
    IEmailService emailService,
    ILogger<ClaimApprovedNotificationHandler> logger) : INotificationHandler<ClaimApprovedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ClaimApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending claim approved notification for claim {ClaimNumber}",
            notification.ClaimNumber);

        await emailService.SendEmailAsync(new EmailMessage(
            To: "client@placeholder.com", // In production, resolve from ClaimId → PolicyId → ClientId → email
            Subject: $"Claim {notification.ClaimNumber} — Approved",
            HtmlBody: $"""
                <h2>Claim Approved</h2>
                <p>Your claim <strong>{notification.ClaimNumber}</strong> has been approved.</p>
                <p>Approved amount: {notification.ApprovedAmount:N2} {notification.Currency}</p>
                <p>Payment will be processed shortly.</p>
                """), cancellationToken);
    }
}

/// <summary>
/// Sends email notification when a claim is denied.
/// </summary>
public sealed class ClaimDeniedNotificationHandler(
    IEmailService emailService,
    ILogger<ClaimDeniedNotificationHandler> logger) : INotificationHandler<ClaimDeniedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ClaimDeniedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending claim denied notification for claim {ClaimNumber}",
            notification.ClaimNumber);

        await emailService.SendEmailAsync(new EmailMessage(
            To: "client@placeholder.com",
            Subject: $"Claim {notification.ClaimNumber} — Denied",
            HtmlBody: $"""
                <h2>Claim Denied</h2>
                <p>Your claim <strong>{notification.ClaimNumber}</strong> has been denied.</p>
                <p>Reason: {notification.Reason}</p>
                <p>If you have questions, please contact your agent.</p>
                """), cancellationToken);
    }
}

/// <summary>
/// Sends email notification when a claim is closed.
/// </summary>
public sealed class ClaimClosedNotificationHandler(
    IEmailService emailService,
    ILogger<ClaimClosedNotificationHandler> logger) : INotificationHandler<ClaimClosedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ClaimClosedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending claim closed notification for claim {ClaimNumber}",
            notification.ClaimNumber);

        await emailService.SendEmailAsync(new EmailMessage(
            To: "client@placeholder.com",
            Subject: $"Claim {notification.ClaimNumber} — Closed",
            HtmlBody: $"""
                <h2>Claim Closed</h2>
                <p>Your claim <strong>{notification.ClaimNumber}</strong> has been closed.</p>
                <p>Reason: {notification.ClosureReason}</p>
                """), cancellationToken);
    }
}
