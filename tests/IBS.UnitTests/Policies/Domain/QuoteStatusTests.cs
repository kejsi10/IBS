using FluentAssertions;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for QuoteStatus and QuoteCarrierStatus.
/// </summary>
public class QuoteStatusTests
{
    [Theory]
    [InlineData(QuoteStatus.Draft, true)]
    [InlineData(QuoteStatus.Submitted, false)]
    [InlineData(QuoteStatus.Quoted, false)]
    [InlineData(QuoteStatus.Accepted, false)]
    [InlineData(QuoteStatus.Expired, false)]
    [InlineData(QuoteStatus.Cancelled, false)]
    public void AllowsCarrierChanges_ReturnsExpected(QuoteStatus status, bool expected)
    {
        // Act & Assert
        status.AllowsCarrierChanges().Should().Be(expected);
    }

    [Theory]
    [InlineData(QuoteStatus.Draft, false)]
    [InlineData(QuoteStatus.Submitted, false)]
    [InlineData(QuoteStatus.Quoted, false)]
    [InlineData(QuoteStatus.Accepted, true)]
    [InlineData(QuoteStatus.Expired, true)]
    [InlineData(QuoteStatus.Cancelled, true)]
    public void IsTerminal_ReturnsExpected(QuoteStatus status, bool expected)
    {
        // Act & Assert
        status.IsTerminal().Should().Be(expected);
    }

    [Theory]
    [InlineData(QuoteStatus.Draft, true)]
    [InlineData(QuoteStatus.Submitted, true)]
    [InlineData(QuoteStatus.Quoted, false)]
    [InlineData(QuoteStatus.Accepted, false)]
    [InlineData(QuoteStatus.Expired, false)]
    [InlineData(QuoteStatus.Cancelled, false)]
    public void CanBeCancelled_ReturnsExpected(QuoteStatus status, bool expected)
    {
        // Act & Assert
        status.CanBeCancelled().Should().Be(expected);
    }

    [Theory]
    [InlineData(QuoteStatus.Draft, "Draft")]
    [InlineData(QuoteStatus.Submitted, "Submitted")]
    [InlineData(QuoteStatus.Quoted, "Quoted")]
    [InlineData(QuoteStatus.Accepted, "Accepted")]
    [InlineData(QuoteStatus.Expired, "Expired")]
    [InlineData(QuoteStatus.Cancelled, "Cancelled")]
    public void GetDisplayName_ReturnsExpected(QuoteStatus status, string expected)
    {
        // Act & Assert
        status.GetDisplayName().Should().Be(expected);
    }

    [Theory]
    [InlineData(QuoteCarrierStatus.Pending, "Pending")]
    [InlineData(QuoteCarrierStatus.Quoted, "Quoted")]
    [InlineData(QuoteCarrierStatus.Declined, "Declined")]
    [InlineData(QuoteCarrierStatus.Expired, "Expired")]
    public void QuoteCarrierStatus_GetDisplayName_ReturnsExpected(QuoteCarrierStatus status, string expected)
    {
        // Act & Assert
        status.GetDisplayName().Should().Be(expected);
    }
}
