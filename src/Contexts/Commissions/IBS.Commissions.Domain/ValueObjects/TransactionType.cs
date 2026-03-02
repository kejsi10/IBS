namespace IBS.Commissions.Domain.ValueObjects;

/// <summary>
/// Represents the type of commission transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>Commission on a new business policy.</summary>
    NewBusiness,

    /// <summary>Commission on a policy renewal.</summary>
    Renewal,

    /// <summary>Commission on a policy endorsement.</summary>
    Endorsement,

    /// <summary>Commission adjustment due to cancellation.</summary>
    Cancellation,

    /// <summary>Chargeback of previously paid commission.</summary>
    Chargeback
}

/// <summary>
/// Extension methods for TransactionType.
/// </summary>
public static class TransactionTypeExtensions
{
    /// <summary>
    /// Gets the display name for a transaction type.
    /// </summary>
    public static string GetDisplayName(this TransactionType type) => type switch
    {
        TransactionType.NewBusiness => "New Business",
        TransactionType.Renewal => "Renewal",
        TransactionType.Endorsement => "Endorsement",
        TransactionType.Cancellation => "Cancellation",
        TransactionType.Chargeback => "Chargeback",
        _ => type.ToString()
    };
}
