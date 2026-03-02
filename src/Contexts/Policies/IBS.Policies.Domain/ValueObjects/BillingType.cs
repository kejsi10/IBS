namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents how a policy is billed.
/// </summary>
public enum BillingType
{
    /// <summary>
    /// Billed directly by the carrier to the insured.
    /// </summary>
    DirectBill = 1,

    /// <summary>
    /// Billed by the agency to the insured.
    /// </summary>
    AgencyBill = 2
}

/// <summary>
/// Represents the payment plan/frequency for a policy.
/// </summary>
public enum PaymentPlan
{
    /// <summary>
    /// Full premium paid at inception.
    /// </summary>
    Annual = 1,

    /// <summary>
    /// Premium paid in two installments.
    /// </summary>
    SemiAnnual = 2,

    /// <summary>
    /// Premium paid in four installments.
    /// </summary>
    Quarterly = 4,

    /// <summary>
    /// Premium paid monthly.
    /// </summary>
    Monthly = 12
}
