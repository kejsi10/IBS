namespace IBS.Claims.Domain.ValueObjects;

/// <summary>
/// Represents the status of a claim payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment has been authorized but not yet issued.</summary>
    Authorized,

    /// <summary>Payment has been issued (check sent, transfer initiated).</summary>
    Issued,

    /// <summary>Payment has been voided/cancelled.</summary>
    Voided
}
