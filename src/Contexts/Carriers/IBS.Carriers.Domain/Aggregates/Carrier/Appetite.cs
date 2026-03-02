using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Aggregates.Carrier;

/// <summary>
/// Represents appetite rules for a carrier - what risks they will accept.
/// </summary>
public sealed class Appetite : Entity
{
    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the line of business this appetite rule applies to.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the states where coverage is available (comma-separated, or "ALL").
    /// </summary>
    public string States { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the minimum years in business required (for commercial lines).
    /// </summary>
    public int? MinYearsInBusiness { get; private set; }

    /// <summary>
    /// Gets the maximum years in business (for new venture programs).
    /// </summary>
    public int? MaxYearsInBusiness { get; private set; }

    /// <summary>
    /// Gets the minimum annual revenue (for commercial lines).
    /// </summary>
    public decimal? MinAnnualRevenue { get; private set; }

    /// <summary>
    /// Gets the maximum annual revenue.
    /// </summary>
    public decimal? MaxAnnualRevenue { get; private set; }

    /// <summary>
    /// Gets the minimum number of employees.
    /// </summary>
    public int? MinEmployees { get; private set; }

    /// <summary>
    /// Gets the maximum number of employees.
    /// </summary>
    public int? MaxEmployees { get; private set; }

    /// <summary>
    /// Gets industries that are accepted (comma-separated SIC/NAICS codes, or "ALL").
    /// </summary>
    public string? AcceptedIndustries { get; private set; }

    /// <summary>
    /// Gets industries that are excluded (comma-separated SIC/NAICS codes).
    /// </summary>
    public string? ExcludedIndustries { get; private set; }

    /// <summary>
    /// Gets additional appetite notes or restrictions.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets whether this appetite rule is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Appetite() { }

    /// <summary>
    /// Creates a new appetite rule.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="states">The states where coverage is available.</param>
    /// <returns>A new Appetite instance.</returns>
    internal static Appetite Create(
        Guid carrierId,
        LineOfBusiness lineOfBusiness,
        string states = "ALL")
    {
        if (string.IsNullOrWhiteSpace(states))
            throw new ArgumentException("States cannot be empty.", nameof(states));

        return new Appetite
        {
            CarrierId = carrierId,
            LineOfBusiness = lineOfBusiness,
            States = states.Trim().ToUpperInvariant(),
            IsActive = true
        };
    }

    /// <summary>
    /// Sets the years in business requirements.
    /// </summary>
    /// <param name="minYears">Minimum years (optional).</param>
    /// <param name="maxYears">Maximum years (optional).</param>
    public void SetYearsInBusinessRequirement(int? minYears, int? maxYears)
    {
        if (minYears.HasValue && minYears.Value < 0)
            throw new ArgumentException("Minimum years cannot be negative.", nameof(minYears));
        if (maxYears.HasValue && maxYears.Value < 0)
            throw new ArgumentException("Maximum years cannot be negative.", nameof(maxYears));
        if (minYears.HasValue && maxYears.HasValue && minYears > maxYears)
            throw new ArgumentException("Minimum years cannot exceed maximum years.");

        MinYearsInBusiness = minYears;
        MaxYearsInBusiness = maxYears;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the annual revenue requirements.
    /// </summary>
    /// <param name="minRevenue">Minimum revenue (optional).</param>
    /// <param name="maxRevenue">Maximum revenue (optional).</param>
    public void SetRevenueRequirement(decimal? minRevenue, decimal? maxRevenue)
    {
        if (minRevenue.HasValue && minRevenue.Value < 0)
            throw new ArgumentException("Minimum revenue cannot be negative.", nameof(minRevenue));
        if (maxRevenue.HasValue && maxRevenue.Value < 0)
            throw new ArgumentException("Maximum revenue cannot be negative.", nameof(maxRevenue));
        if (minRevenue.HasValue && maxRevenue.HasValue && minRevenue > maxRevenue)
            throw new ArgumentException("Minimum revenue cannot exceed maximum revenue.");

        MinAnnualRevenue = minRevenue;
        MaxAnnualRevenue = maxRevenue;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the employee count requirements.
    /// </summary>
    /// <param name="minEmployees">Minimum employees (optional).</param>
    /// <param name="maxEmployees">Maximum employees (optional).</param>
    public void SetEmployeeRequirement(int? minEmployees, int? maxEmployees)
    {
        if (minEmployees.HasValue && minEmployees.Value < 0)
            throw new ArgumentException("Minimum employees cannot be negative.", nameof(minEmployees));
        if (maxEmployees.HasValue && maxEmployees.Value < 0)
            throw new ArgumentException("Maximum employees cannot be negative.", nameof(maxEmployees));
        if (minEmployees.HasValue && maxEmployees.HasValue && minEmployees > maxEmployees)
            throw new ArgumentException("Minimum employees cannot exceed maximum employees.");

        MinEmployees = minEmployees;
        MaxEmployees = maxEmployees;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the industry restrictions.
    /// </summary>
    /// <param name="accepted">Accepted industries (comma-separated codes, or "ALL").</param>
    /// <param name="excluded">Excluded industries (comma-separated codes).</param>
    public void SetIndustryRestrictions(string? accepted, string? excluded)
    {
        AcceptedIndustries = accepted?.Trim().ToUpperInvariant();
        ExcludedIndustries = excluded?.Trim().ToUpperInvariant();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the states where coverage is available.
    /// </summary>
    /// <param name="states">The states (comma-separated, or "ALL").</param>
    public void UpdateStates(string states)
    {
        if (string.IsNullOrWhiteSpace(states))
            throw new ArgumentException("States cannot be empty.", nameof(states));

        States = states.Trim().ToUpperInvariant();
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets additional notes for this appetite rule.
    /// </summary>
    /// <param name="notes">The notes.</param>
    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates this appetite rule.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates this appetite rule.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if a state is covered by this appetite rule.
    /// </summary>
    /// <param name="state">The state code (e.g., "CA", "TX").</param>
    /// <returns>True if the state is covered; otherwise, false.</returns>
    public bool CoversState(string state)
    {
        if (!IsActive) return false;
        if (States == "ALL") return true;

        var stateList = States.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return stateList.Contains(state.Trim());
    }
}
