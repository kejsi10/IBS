using FluentAssertions;
using IBS.Commissions.Application.Queries.GetProducerReport;
using IBS.Commissions.Domain.Queries;
using NSubstitute;

namespace IBS.UnitTests.Commissions.Application;

/// <summary>
/// Unit tests for the GetProducerReportQueryHandler.
/// </summary>
public class GetProducerReportQueryHandlerTests
{
    private readonly ICommissionStatementQueries _queries = Substitute.For<ICommissionStatementQueries>();
    private readonly GetProducerReportQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProducerReportQueryHandlerTests"/> class.
    /// </summary>
    public GetProducerReportQueryHandlerTests()
    {
        _handler = new GetProducerReportQueryHandler(_queries);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesFiltersToQuery()
    {
        // Arrange
        var producerId = Guid.NewGuid();
        var query = new GetProducerReportQuery(Guid.NewGuid(), producerId, 3, 2025);
        _queries.GetProducerReportAsync(producerId, 3, 2025, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerReportEntry>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queries.Received(1).GetProducerReportAsync(producerId, 3, 2025, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleEntries_ReturnsMappedDtos()
    {
        // Arrange
        var producerId1 = Guid.NewGuid();
        var producerId2 = Guid.NewGuid();
        var entries = new List<ProducerReportEntry>
        {
            new(producerId1, "Producer A", 1, 2025, 10, 8_000m, 50.0m, "USD"),
            new(producerId2, "Producer B", 1, 2025, 5, 3_000m, 30.0m, "USD")
        };
        _queries.GetProducerReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetProducerReportQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var first = result.Value[0];
        first.ProducerId.Should().Be(producerId1);
        first.ProducerName.Should().Be("Producer A");
        first.PeriodMonth.Should().Be(1);
        first.PeriodYear.Should().Be(2025);
        first.LineItemCount.Should().Be(10);
        first.TotalSplitAmount.Should().Be(8_000m);
        first.AverageSplitPercentage.Should().Be(50.0m);
        first.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_NoEntries_ReturnsEmptyList()
    {
        // Arrange
        _queries.GetProducerReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerReportEntry>());

        var query = new GetProducerReportQuery(Guid.NewGuid());

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
        var query = new GetProducerReportQuery(Guid.NewGuid());
        _queries.GetProducerReportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerReportEntry>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queries.Received(1).GetProducerReportAsync(null, null, null, Arg.Any<CancellationToken>());
    }
}
