using FluentAssertions;
using IBS.Carriers.Domain.ValueObjects;
using IBS.IntegrationTests.Fixtures;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using IBS.Policies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Policies;

/// <summary>
/// Integration tests for QuoteRepository and QuoteQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class QuoteRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private QuoteTestDbContext _context = null!;
    private QuoteRepository _repository = null!;
    private QuoteQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public QuoteRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<QuoteTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"QuoteTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new QuoteTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new QuoteRepository(_context);
        _queries = new QuoteQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewQuote_PersistsQuote()
    {
        // Arrange
        var quote = CreateQuote();

        // Act
        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(quote.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ClientId.Should().Be(quote.ClientId);
        retrieved.Status.Should().Be(QuoteStatus.Draft);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingQuote_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_QuoteWithCarriers_ReturnsCarriers()
    {
        // Arrange
        var quote = CreateQuote();
        quote.AddCarrier(Guid.NewGuid());
        quote.AddCarrier(Guid.NewGuid());

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Carriers.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_AddCarrier_PersistsChanges()
    {
        // Arrange
        var quote = CreateQuote();
        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);
        retrieved!.AddCarrier(Guid.NewGuid());
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(quote.Id);
        updated!.Carriers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetExpiredQuotesAsync_ExpiredQuotes_ReturnsMatching()
    {
        // Arrange
        var expiredQuote = CreateQuote(expiresAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)));
        var activeQuote = CreateQuote(expiresAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

        await _repository.AddAsync(expiredQuote);
        await _repository.AddAsync(activeQuote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetExpiredQuotesAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(expiredQuote.Id);
    }

    [Fact]
    public async Task GetExpiredQuotesAsync_NotExpired_ReturnsEmpty()
    {
        // Arrange
        var activeQuote = CreateQuote(expiresAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

        await _repository.AddAsync(activeQuote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetExpiredQuotesAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Queries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var quote = CreateQuote();
        quote.AddCarrier(Guid.NewGuid());

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByIdAsync(_tenantId, quote.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(quote.Id);
        result.Carriers.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_ReturnsPagedResults()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            var quote = CreateQuote();
            await _repository.AddAsync(quote);
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new QuoteSearchFilter { PageNumber = 1, PageSize = 3 };
        var result = await _queries.SearchAsync(_tenantId, filter);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Quotes.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Queries_GetByClientIdAsync_FiltersCorrectly()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var quote1 = CreateQuote(clientId: clientId);
        var quote2 = CreateQuote(clientId: clientId);
        var otherQuote = CreateQuote(clientId: Guid.NewGuid());

        await _repository.AddAsync(quote1);
        await _repository.AddAsync(quote2);
        await _repository.AddAsync(otherQuote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByClientIdAsync(_tenantId, clientId, 1, 20);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Queries_GetSummaryAsync_ReturnsCounts()
    {
        // Arrange
        var quote1 = CreateQuote();
        var quote2 = CreateQuote();
        quote2.AddCarrier(Guid.NewGuid());
        quote2.Submit();

        await _repository.AddAsync(quote1);
        await _repository.AddAsync(quote2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetSummaryAsync(_tenantId);

        // Assert
        result.TotalQuotes.Should().Be(2);
        result.DraftCount.Should().Be(1);
        result.SubmittedCount.Should().Be(1);
    }

    [Fact]
    public async Task SubmitQuote_PersistsStatusChange()
    {
        // Arrange
        var quote = CreateQuote();
        quote.AddCarrier(Guid.NewGuid());
        quote.Submit();

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(QuoteStatus.Submitted);
    }

    [Fact]
    public async Task RecordQuotedResponse_PersistsCarrierResponse()
    {
        // Arrange
        var quote = CreateQuote();
        var carrierId = Guid.NewGuid();
        var carrier = quote.AddCarrier(carrierId);
        quote.Submit();
        quote.RecordQuotedResponse(carrier.Id, 7500m, "USD", conditions: "Standard terms", proposedCoverages: "GL Full");

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(QuoteStatus.Quoted);
        var quotedCarrier = retrieved.Carriers.First(c => c.CarrierId == carrierId);
        quotedCarrier.PremiumAmount.Should().Be(7500m);
        quotedCarrier.PremiumCurrency.Should().Be("USD");
        quotedCarrier.Conditions.Should().Be("Standard terms");
        quotedCarrier.ProposedCoverages.Should().Be("GL Full");
        quotedCarrier.Status.Should().Be(QuoteCarrierStatus.Quoted);
    }

    [Fact]
    public async Task AcceptQuote_PersistsAcceptedState()
    {
        // Arrange
        var quote = CreateQuote();
        var carrierId = Guid.NewGuid();
        var carrier = quote.AddCarrier(carrierId);
        quote.Submit();
        quote.RecordQuotedResponse(carrier.Id, 7500m);
        var policyId = Guid.NewGuid();
        quote.Accept(carrier.Id, policyId);

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(QuoteStatus.Accepted);
        retrieved.AcceptedCarrierId.Should().Be(carrierId);
        retrieved.PolicyId.Should().Be(policyId);
    }

    [Fact]
    public async Task RemoveCarrier_PersistsChanges()
    {
        // Arrange
        var quote = CreateQuote();
        var carrier1 = quote.AddCarrier(Guid.NewGuid());
        quote.AddCarrier(Guid.NewGuid());

        await _repository.AddAsync(quote);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(quote.Id);
        retrieved!.RemoveCarrier(carrier1.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(quote.Id);
        updated.Should().NotBeNull();
        updated!.Carriers.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_WithStatusFilter_ReturnsFiltered()
    {
        // Arrange
        var draftQuote = CreateQuote();

        var submittedQuote = CreateQuote();
        submittedQuote.AddCarrier(Guid.NewGuid());
        submittedQuote.Submit();

        await _repository.AddAsync(draftQuote);
        await _repository.AddAsync(submittedQuote);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new QuoteSearchFilter { Status = QuoteStatus.Submitted, PageNumber = 1, PageSize = 10 };
        var result = await _queries.SearchAsync(_tenantId, filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Quotes.Should().HaveCount(1);
        result.Quotes[0].Status.Should().Be("Submitted");
    }

    private Quote CreateQuote(Guid? clientId = null, DateOnly? expiresAt = null)
    {
        return Quote.Create(
            _tenantId,
            clientId ?? Guid.NewGuid(),
            LineOfBusiness.GeneralLiability,
            EffectivePeriod.Annual(DateOnly.FromDateTime(DateTime.UtcNow)),
            _userId,
            notes: "Test quote",
            expiresAt: expiresAt);
    }
}

/// <summary>
/// Test DbContext for Quote integration tests.
/// </summary>
public class QuoteTestDbContext : DbContext
{
    public QuoteTestDbContext(DbContextOptions<QuoteTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the quotes set.
    /// </summary>
    public DbSet<Quote> Quotes => Set<Quote>();

    /// <summary>
    /// Gets the quote carriers set.
    /// </summary>
    public DbSet<QuoteCarrier> QuoteCarriers => Set<QuoteCarrier>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Quote configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Policies.Infrastructure.Persistence.Configurations.QuoteConfiguration).Assembly);
    }
}
