using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Domain.Aggregates.CommissionStatement;

/// <summary>
/// Represents a producer's share of a commission line item.
/// </summary>
public sealed class ProducerSplit : Entity
{
    /// <summary>
    /// Gets the statement identifier.
    /// </summary>
    public Guid StatementId { get; private set; }

    /// <summary>
    /// Gets the line item identifier.
    /// </summary>
    public Guid LineItemId { get; private set; }

    /// <summary>
    /// Gets the producer name.
    /// </summary>
    public string ProducerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the producer identifier.
    /// </summary>
    public Guid ProducerId { get; private set; }

    /// <summary>
    /// Gets the split percentage (0-100%).
    /// </summary>
    public decimal SplitPercentage { get; private set; }

    /// <summary>
    /// Gets the split amount.
    /// </summary>
    public Money SplitAmount { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private ProducerSplit() { }

    /// <summary>
    /// Creates a new producer split.
    /// </summary>
    /// <param name="statementId">The statement identifier.</param>
    /// <param name="lineItemId">The line item identifier.</param>
    /// <param name="producerName">The producer name.</param>
    /// <param name="producerId">The producer identifier.</param>
    /// <param name="splitPercentage">The split percentage.</param>
    /// <param name="splitAmount">The split amount.</param>
    /// <returns>A new ProducerSplit.</returns>
    public static ProducerSplit Create(
        Guid statementId,
        Guid lineItemId,
        string producerName,
        Guid producerId,
        decimal splitPercentage,
        Money splitAmount)
    {
        if (string.IsNullOrWhiteSpace(producerName))
            throw new ArgumentException("Producer name is required.", nameof(producerName));

        if (splitPercentage <= 0 || splitPercentage > 100)
            throw new BusinessRuleViolationException($"Split percentage must be between 0 and 100%. Got: {splitPercentage}");

        return new ProducerSplit
        {
            StatementId = statementId,
            LineItemId = lineItemId,
            ProducerName = producerName.Trim(),
            ProducerId = producerId,
            SplitPercentage = Math.Round(splitPercentage, 4),
            SplitAmount = splitAmount
        };
    }
}
