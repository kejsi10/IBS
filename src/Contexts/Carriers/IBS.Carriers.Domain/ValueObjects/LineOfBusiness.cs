namespace IBS.Carriers.Domain.ValueObjects;

/// <summary>
/// Represents the line of business for insurance products.
/// </summary>
public enum LineOfBusiness
{
    // Personal Lines
    /// <summary>Personal auto insurance.</summary>
    PersonalAuto,

    /// <summary>Homeowners insurance.</summary>
    Homeowners,

    /// <summary>Renters insurance.</summary>
    Renters,

    /// <summary>Personal umbrella/excess liability.</summary>
    PersonalUmbrella,

    /// <summary>Life insurance.</summary>
    Life,

    /// <summary>Health insurance.</summary>
    Health,

    // Commercial Lines
    /// <summary>General liability insurance.</summary>
    GeneralLiability,

    /// <summary>Commercial property insurance.</summary>
    CommercialProperty,

    /// <summary>Workers compensation insurance.</summary>
    WorkersCompensation,

    /// <summary>Commercial auto insurance.</summary>
    CommercialAuto,

    /// <summary>Professional liability (E&O) insurance.</summary>
    ProfessionalLiability,

    /// <summary>Directors & Officers (D&O) insurance.</summary>
    DirectorsAndOfficers,

    /// <summary>Cyber liability insurance.</summary>
    CyberLiability,

    /// <summary>Business Owners Policy (BOP).</summary>
    BusinessOwnersPolicy,

    /// <summary>Commercial umbrella/excess liability.</summary>
    CommercialUmbrella,

    /// <summary>Inland marine insurance.</summary>
    InlandMarine,

    /// <summary>Surety bonds.</summary>
    Surety
}

/// <summary>
/// Extension methods for LineOfBusiness.
/// </summary>
public static class LineOfBusinessExtensions
{
    /// <summary>
    /// Determines if the line of business is a personal line.
    /// </summary>
    public static bool IsPersonalLine(this LineOfBusiness lob) => lob switch
    {
        LineOfBusiness.PersonalAuto => true,
        LineOfBusiness.Homeowners => true,
        LineOfBusiness.Renters => true,
        LineOfBusiness.PersonalUmbrella => true,
        LineOfBusiness.Life => true,
        LineOfBusiness.Health => true,
        _ => false
    };

    /// <summary>
    /// Determines if the line of business is a commercial line.
    /// </summary>
    public static bool IsCommercialLine(this LineOfBusiness lob) => !lob.IsPersonalLine();

    /// <summary>
    /// Gets a display-friendly name for the line of business.
    /// </summary>
    public static string GetDisplayName(this LineOfBusiness lob) => lob switch
    {
        LineOfBusiness.PersonalAuto => "Personal Auto",
        LineOfBusiness.Homeowners => "Homeowners",
        LineOfBusiness.Renters => "Renters",
        LineOfBusiness.PersonalUmbrella => "Personal Umbrella",
        LineOfBusiness.Life => "Life",
        LineOfBusiness.Health => "Health",
        LineOfBusiness.GeneralLiability => "General Liability",
        LineOfBusiness.CommercialProperty => "Commercial Property",
        LineOfBusiness.WorkersCompensation => "Workers Compensation",
        LineOfBusiness.CommercialAuto => "Commercial Auto",
        LineOfBusiness.ProfessionalLiability => "Professional Liability (E&O)",
        LineOfBusiness.DirectorsAndOfficers => "Directors & Officers (D&O)",
        LineOfBusiness.CyberLiability => "Cyber Liability",
        LineOfBusiness.BusinessOwnersPolicy => "Business Owners Policy (BOP)",
        LineOfBusiness.CommercialUmbrella => "Commercial Umbrella",
        LineOfBusiness.InlandMarine => "Inland Marine",
        LineOfBusiness.Surety => "Surety Bonds",
        _ => lob.ToString()
    };
}
