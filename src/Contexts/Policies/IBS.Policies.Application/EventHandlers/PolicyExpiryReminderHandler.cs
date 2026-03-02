using IBS.BuildingBlocks.Application;
using IBS.Policies.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IBS.Policies.Application.EventHandlers;

/// <summary>
/// Sends email notification when a policy expires.
/// </summary>
public sealed class PolicyExpiredNotificationHandler(
    IEmailService emailService,
    ILogger<PolicyExpiredNotificationHandler> logger) : INotificationHandler<PolicyExpiredEvent>
{
    /// <inheritdoc />
    public async Task Handle(PolicyExpiredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending policy expiry notification for policy {PolicyNumber} (expired {ExpirationDate})",
            notification.PolicyNumber,
            notification.ExpirationDate);

        await emailService.SendEmailAsync(new EmailMessage(
            To: "client@placeholder.com", // In production, resolve from PolicyId → ClientId → email
            Subject: $"Policy {notification.PolicyNumber} — Expired",
            HtmlBody: $"""
                <h2>Policy Expired</h2>
                <p>Your policy <strong>{notification.PolicyNumber}</strong> has expired on {notification.ExpirationDate:d}.</p>
                <p>Please contact your agent to discuss renewal options.</p>
                """), cancellationToken);
    }
}

/// <summary>
/// Sends email notification when a policy is cancelled.
/// </summary>
public sealed class PolicyCancelledNotificationHandler(
    IEmailService emailService,
    ILogger<PolicyCancelledNotificationHandler> logger) : INotificationHandler<PolicyCancelledEvent>
{
    /// <inheritdoc />
    public async Task Handle(PolicyCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending policy cancelled notification for policy {PolicyNumber}",
            notification.PolicyNumber);

        await emailService.SendEmailAsync(new EmailMessage(
            To: "client@placeholder.com",
            Subject: $"Policy {notification.PolicyNumber} — Cancelled",
            HtmlBody: $"""
                <h2>Policy Cancelled</h2>
                <p>Your policy <strong>{notification.PolicyNumber}</strong> has been cancelled effective {notification.CancellationDate:d}.</p>
                <p>Reason: {notification.Reason}</p>
                <p>Please contact your agent if you have questions.</p>
                """), cancellationToken);
    }
}
