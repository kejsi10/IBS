using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Aggregates.Policy;

/// <summary>
/// Represents a coverage within a policy.
/// </summary>
public sealed class Coverage : Entity
{
    /// <summary>
    /// Gets the policy identifier.
    /// </summary>
    public Guid PolicyId { get; private set; }

    /// <summary>
    /// Gets the coverage code (e.g., "BI", "PD", "COLL").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the coverage name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the coverage description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the coverage limit amount.
    /// </summary>
    public Money? LimitAmount { get; private set; }

    /// <summary>
    /// Gets the per-occurrence limit (if applicable).
    /// </summary>
    public Money? PerOccurrenceLimit { get; private set; }

    /// <summary>
    /// Gets the aggregate limit (if applicable).
    /// </summary>
    public Money? AggregateLimit { get; private set; }

    /// <summary>
    /// Gets the deductible amount.
    /// </summary>
    public Money? DeductibleAmount { get; private set; }

    /// <summary>
    /// Gets the premium amount for this coverage.
    /// </summary>
    public Money PremiumAmount { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether this coverage is optional.
    /// </summary>
    public bool IsOptional { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this coverage is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the effective date for this coverage (may differ from policy).
    /// </summary>
    public DateOnly? EffectiveDate { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Coverage() { }

    /// <summary>
    /// Creates a new coverage.
    /// </summary>
    internal static Coverage Create(
        Guid policyId,
        string code,
        string name,
        Money premium,
        string? description = null,
        Money? limitAmount = null,
        Money? deductibleAmount = null,
        bool isOptional = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Coverage code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Coverage name is required.", nameof(name));

        return new Coverage
        {
            PolicyId = policyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            LimitAmount = limitAmount,
            DeductibleAmount = deductibleAmount,
            PremiumAmount = premium,
            IsOptional = isOptional,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the coverage details.
    /// </summary>
    internal void Update(
        string name,
        string? description,
        Money? limitAmount,
        Money? perOccurrenceLimit,
        Money? aggregateLimit,
        Money? deductibleAmount,
        Money premium)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Coverage name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        LimitAmount = limitAmount;
        PerOccurrenceLimit = perOccurrenceLimit;
        AggregateLimit = aggregateLimit;
        DeductibleAmount = deductibleAmount;
        PremiumAmount = premium;
    }

    /// <summary>
    /// Updates just the premium amount.
    /// </summary>
    internal void UpdatePremium(Money newPremium)
    {
        PremiumAmount = newPremium;
    }

    /// <summary>
    /// Sets the limits for this coverage.
    /// </summary>
    internal void SetLimits(Money? perOccurrence, Money? aggregate)
    {
        PerOccurrenceLimit = perOccurrence;
        AggregateLimit = aggregate;
    }

    /// <summary>
    /// Deactivates this coverage.
    /// </summary>
    internal void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates this coverage.
    /// </summary>
    internal void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Sets the effective date for this coverage.
    /// </summary>
    internal void SetEffectiveDate(DateOnly effectiveDate)
    {
        EffectiveDate = effectiveDate;
    }
}
