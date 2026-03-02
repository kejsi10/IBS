using FluentAssertions;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Application.Queries.GetCommissionStatistics;
using IBS.Commissions.Domain.Queries;
using NSubstitute;

namespace IBS.UnitTests.Commissions.Application;

/// <summary>
/// Unit tests for the GetCommissionStatisticsQueryHandler.
/// </summary>
public class GetCommissionStatisticsQueryHandlerTests
{
    private readonly ICommissionStatementQueries _queries = Substitute.For<ICommissionStatementQueries>();
    private readonly GetCommissionStatisticsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommissionStatisticsQueryHandlerTests"/> class.
    /// </summary>
    public GetCommissionStatisticsQueryHandlerTests()
    {
        _handler = new GetCommissionStatisticsQueryHandler(_queries);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsStatistics()
    {
        // Arrange
        var stats = new CommissionStatistics(
            TotalStatements: 50,
            ReceivedStatements: 20,
            ReconcilingStatements: 10,
            ReconciledStatements: 8,
            PaidStatements: 7,
            DisputedStatements: 5,
            TotalCommissionAmount: 100_000m,
            TotalPaidAmount: 70_000m,
            TotalDisputedAmount: 5_000m,
            StatementsByStatus: new Dictionary<string, int> { ["Received"] = 20, ["Paid"] = 7 },
            CommissionByCarrier: new Dictionary<string, decimal> { ["Carrier A"] = 60_000m, ["Carrier B"] = 40_000m }
        );
        _queries.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(stats);

        var query = new GetCommissionStatisticsQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.TotalStatements.Should().Be(50);
        dto.ReceivedStatements.Should().Be(20);
        dto.ReconcilingStatements.Should().Be(10);
        dto.ReconciledStatements.Should().Be(8);
        dto.PaidStatements.Should().Be(7);
        dto.DisputedStatements.Should().Be(5);
        dto.TotalCommissionAmount.Should().Be(100_000m);
        dto.TotalPaidAmount.Should().Be(70_000m);
        dto.TotalDisputedAmount.Should().Be(5_000m);
        dto.StatementsByStatus.Should().HaveCount(2);
        dto.CommissionByCarrier.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ZeroCounts_ReturnsZeroStats()
    {
        // Arrange
        var stats = new CommissionStatistics(
            TotalStatements: 0,
            ReceivedStatements: 0,
            ReconcilingStatements: 0,
            ReconciledStatements: 0,
            PaidStatements: 0,
            DisputedStatements: 0,
            TotalCommissionAmount: 0m,
            TotalPaidAmount: 0m,
            TotalDisputedAmount: 0m,
            StatementsByStatus: new Dictionary<string, int>(),
            CommissionByCarrier: new Dictionary<string, decimal>()
        );
        _queries.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(stats);

        var query = new GetCommissionStatisticsQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.TotalStatements.Should().Be(0);
        dto.TotalCommissionAmount.Should().Be(0m);
        dto.StatementsByStatus.Should().BeEmpty();
        dto.CommissionByCarrier.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsGetStatisticsAsync()
    {
        // Arrange
        var stats = new CommissionStatistics(
            TotalStatements: 1,
            ReceivedStatements: 1,
            ReconcilingStatements: 0,
            ReconciledStatements: 0,
            PaidStatements: 0,
            DisputedStatements: 0,
            TotalCommissionAmount: 500m,
            TotalPaidAmount: 0m,
            TotalDisputedAmount: 0m,
            StatementsByStatus: new Dictionary<string, int>(),
            CommissionByCarrier: new Dictionary<string, decimal>()
        );
        _queries.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(stats);

        var query = new GetCommissionStatisticsQuery(Guid.NewGuid());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queries.Received(1).GetStatisticsAsync(Arg.Any<CancellationToken>());
    }
}
