using FluentAssertions;
using IBS.Commissions.Application.Queries.GetCommissionSummaryReport;
using IBS.Commissions.Domain.Queries;
using NSubstitute;

namespace IBS.UnitTests.Commissions.Application;

/// <summary>
/// Unit tests for the GetCommissionSummaryReportQueryHandler.
/// </summary>
public class GetCommissionSummaryReportQueryHandlerTests
{
    private readonly ICommissionStatementQueries _queries = Substitute.For<ICommissionStatementQueries>();
    private readonly GetCommissionSummaryReportQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommissionSummaryReportQueryHandlerTests"/> class.
    /// </summary>
    public GetCommissionSummaryReportQueryHandlerTests()
    {
        _handler = new GetCommissionSummaryReportQueryHandler(_queries);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesFiltersToQuery()
    {
        // Arrange
        var carrierId = Guid.NewGuid();
        var query = new GetCommissionSummaryReportQuery(Guid.NewGuid(), carrierId, 6, 2025);
        _queries.GetSummaryReportAsync(carrierId, 6, 2025, Arg.Any<CancellationToken>())
            .Returns(new List<CommissionSummaryEntry>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queries.Received(1).GetSummaryReportAsync(carrierId, 6, 2025, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleEntries_ReturnsMappedDtos()
    {
        // Arrange
        var carrierId1 = Guid.NewGuid();
        var carrierId2 = Guid.NewGuid();
        var entries = new List<CommissionSummaryEntry>
        {
            new(carrierId1, "Carrier A", 1, 2025, 5, 50_000m, 5_000m, 4_000m, "USD"),
            new(carrierId2, "Carrier B", 1, 2025, 3, 30_000m, 3_000m, 2_500m, "USD")
        };
        _queries.GetSummaryReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCommissionSummaryReportQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var first = result.Value[0];
        first.CarrierId.Should().Be(carrierId1);
        first.CarrierName.Should().Be("Carrier A");
        first.PeriodMonth.Should().Be(1);
        first.PeriodYear.Should().Be(2025);
        first.StatementCount.Should().Be(5);
        first.TotalPremium.Should().Be(50_000m);
        first.TotalCommission.Should().Be(5_000m);
        first.TotalPaid.Should().Be(4_000m);
        first.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_NoEntries_ReturnsEmptyList()
    {
        // Arrange
        _queries.GetSummaryReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<CommissionSummaryEntry>());

        var query = new GetCommissionSummaryReportQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullFilters_PassesNullsToQuery()
    {
        // Arrange
        var query = new GetCommissionSummaryReportQuery(Guid.NewGuid());
        _queries.GetSummaryReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<CommissionSummaryEntry>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queries.Received(1).GetSummaryReportAsync(null, null, null, Arg.Any<CancellationToken>());
    }
}
