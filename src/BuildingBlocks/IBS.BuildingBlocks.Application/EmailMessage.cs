namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
/// <param name="To">The recipient email address.</param>
/// <param name="Subject">The email subject.</param>
/// <param name="HtmlBody">The HTML body content.</param>
/// <param name="Cc">Optional CC recipients.</param>
/// <param name="Bcc">Optional BCC recipients.</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    IReadOnlyList<string>? Cc = null,
    IReadOnlyList<string>? Bcc = null);
