using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Events;

namespace IBS.Commissions.Domain.Aggregates.CommissionSchedule;

/// <summary>
/// Represents a commission rate schedule for a carrier and line of business.
/// This is the aggregate root for the CommissionSchedule aggregate.
/// </summary>
public sealed class CommissionSchedule : TenantAggregateRoot
{
    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the carrier name.
    /// </summary>
    public string CarrierName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public string LineOfBusiness { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the new business commission rate (0-100%).
    /// </summary>
    public decimal NewBusinessRate { get; private set; }

    /// <summary>
    /// Gets the renewal commission rate (0-100%).
    /// </summary>
    public decimal RenewalRate { get; private set; }

    /// <summary>
    /// Gets the effective start date.
    /// </summary>
    public DateOnly EffectiveFrom { get; private set; }

    /// <summary>
    /// Gets the effective end date.
    /// </summary>
    public DateOnly? EffectiveTo { get; private set; }

    /// <summary>
    /// Gets whether the schedule is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private CommissionSchedule() { }

    /// <summary>
    /// Creates a new commission schedule.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="carrierName">The carrier name.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="newBusinessRate">The new business commission rate.</param>
    /// <param name="renewalRate">The renewal commission rate.</param>
    /// <param name="effectiveFrom">The effective start date.</param>
    /// <param name="effectiveTo">The optional effective end date.</param>
    /// <returns>A new CommissionSchedule instance.</returns>
    public static CommissionSchedule Create(
        Guid tenantId,
        Guid carrierId,
        string carrierName,
        string lineOfBusiness,
        decimal newBusinessRate,
        decimal renewalRate,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo = null)
    {
        ValidateRate(newBusinessRate, nameof(newBusinessRate));
        ValidateRate(renewalRate, nameof(renewalRate));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name is required.", nameof(carrierName));

        if (string.IsNullOrWhiteSpace(lineOfBusiness))
            throw new ArgumentException("Line of business is required.", nameof(lineOfBusiness));

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new BusinessRuleViolationException("Effective end date cannot be before start date.");

        var schedule = new CommissionSchedule
        {
            TenantId = tenantId,
            CarrierId = carrierId,
            CarrierName = carrierName.Trim(),
            LineOfBusiness = lineOfBusiness.Trim(),
            NewBusinessRate = Math.Round(newBusinessRate, 4),
            RenewalRate = Math.Round(renewalRate, 4),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true
        };

        schedule.RaiseDomainEvent(new ScheduleCreatedEvent(
            schedule.Id,
            schedule.TenantId,
            schedule.CarrierId,
            schedule.CarrierName,
            schedule.LineOfBusiness));

        return schedule;
    }

    /// <summary>
    /// Updates the commission schedule.
    /// </summary>
    /// <param name="carrierName">The carrier name.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="newBusinessRate">The new business rate.</param>
    /// <param name="renewalRate">The renewal rate.</param>
    /// <param name="effectiveFrom">The effective start date.</param>
    /// <param name="effectiveTo">The optional effective end date.</param>
    public void Update(
        string carrierName,
        string lineOfBusiness,
        decimal newBusinessRate,
        decimal renewalRate,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo = null)
    {
        ValidateRate(newBusinessRate, nameof(newBusinessRate));
        ValidateRate(renewalRate, nameof(renewalRate));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name is required.", nameof(carrierName));

        if (string.IsNullOrWhiteSpace(lineOfBusiness))
            throw new ArgumentException("Line of business is required.", nameof(lineOfBusiness));

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new BusinessRuleViolationException("Effective end date cannot be before start date.");

        CarrierName = carrierName.Trim();
        LineOfBusiness = lineOfBusiness.Trim();
        NewBusinessRate = Math.Round(newBusinessRate, 4);
        RenewalRate = Math.Round(renewalRate, 4);
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the commission schedule.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Schedule is already inactive.");

        IsActive = false;
        MarkAsUpdated();
    }

    private static void ValidateRate(decimal rate, string paramName)
    {
        if (rate < 0 || rate > 100)
            throw new BusinessRuleViolationException($"Commission rate must be between 0 and 100%. Got: {rate}");
    }
}
