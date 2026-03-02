namespace IBS.Claims.Domain.ValueObjects;

/// <summary>
/// Represents the type of loss for a claim.
/// </summary>
public enum LossType
{
    /// <summary>Physical property damage.</summary>
    PropertyDamage,

    /// <summary>Third-party liability claim.</summary>
    Liability,

    /// <summary>Workers' compensation claim.</summary>
    WorkersComp,

    /// <summary>Automobile-related claim.</summary>
    Auto,

    /// <summary>Professional liability (errors and omissions).</summary>
    Professional,

    /// <summary>Cyber/data breach claim.</summary>
    Cyber,

    /// <summary>Natural disaster (flood, earthquake, hurricane, etc.).</summary>
    NaturalDisaster,

    /// <summary>Theft or fraud claim.</summary>
    TheftFraud,

    /// <summary>Bodily injury claim.</summary>
    BodilyInjury,

    /// <summary>Other/unclassified loss type.</summary>
    Other
}

/// <summary>
/// Extension methods for LossType.
/// </summary>
public static class LossTypeExtensions
{
    /// <summary>
    /// Gets the display name for a loss type.
    /// </summary>
    public static string GetDisplayName(this LossType lossType) => lossType switch
    {
        LossType.PropertyDamage => "Property Damage",
        LossType.Liability => "Liability",
        LossType.WorkersComp => "Workers' Compensation",
        LossType.Auto => "Auto",
        LossType.Professional => "Professional Liability",
        LossType.Cyber => "Cyber",
        LossType.NaturalDisaster => "Natural Disaster",
        LossType.TheftFraud => "Theft/Fraud",
        LossType.BodilyInjury => "Bodily Injury",
        LossType.Other => "Other",
        _ => lossType.ToString()
    };
}
