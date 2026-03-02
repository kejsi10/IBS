using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Aggregates.Claim;

/// <summary>
/// Represents a reserve set on a claim.
/// </summary>
public sealed class Reserve : Entity
{
    /// <summary>
    /// Gets the claim identifier.
    /// </summary>
    public Guid ClaimId { get; private set; }

    /// <summary>
    /// Gets the reserve type (e.g., "Indemnity", "Expense", "Legal").
    /// </summary>
    public string ReserveType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the reserve amount.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Gets the identifier of the user who set the reserve.
    /// </summary>
    public string SetBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date/time when the reserve was set.
    /// </summary>
    public DateTimeOffset SetAt { get; private set; }

    /// <summary>
    /// Gets any notes about the reserve.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Reserve() { }

    /// <summary>
    /// Creates a new reserve.
    /// </summary>
    /// <param name="claimId">The claim identifier.</param>
    /// <param name="reserveType">The reserve type.</param>
    /// <param name="amount">The reserve amount.</param>
    /// <param name="setBy">The user who set the reserve.</param>
    /// <param name="notes">Optional notes.</param>
    /// <returns>A new Reserve instance.</returns>
    public static Reserve Create(Guid claimId, string reserveType, Money amount, string setBy, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(reserveType))
            throw new ArgumentException("Reserve type is required.", nameof(reserveType));

        if (amount.Amount <= 0)
            throw new ArgumentException("Reserve amount must be positive.", nameof(amount));

        if (string.IsNullOrWhiteSpace(setBy))
            throw new ArgumentException("Set by is required.", nameof(setBy));

        return new Reserve
        {
            ClaimId = claimId,
            ReserveType = reserveType.Trim(),
            Amount = amount,
            SetBy = setBy.Trim(),
            SetAt = DateTimeOffset.UtcNow,
            Notes = notes?.Trim()
        };
    }
}
