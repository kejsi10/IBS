using FluentAssertions;
using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Carriers.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Carriers;

/// <summary>
/// Integration tests for CarrierRepository using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class CarrierRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private TestDbContext _context = null!;
    private CarrierRepository _repository = null!;
    private CarrierQueries _queries = null!;

    public CarrierRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"CarrierTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new TestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new CarrierRepository(_context);
        _queries = new CarrierQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewCarrier_PersistsCarrier()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));

        // Act
        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(carrier.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Carrier");
        retrieved.Code.Value.Should().Be("TEST");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCarrier_ReturnsCarrierWithChildEntities()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));
        carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(carrier.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Products.Should().HaveCount(1);
        retrieved.Appetites.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCarrier_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCarrier()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TRAV"));
        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByCodeAsync("TRAV");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(carrier.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleCarriers_ReturnsAllOrderedByName()
    {
        // Arrange
        var carrier1 = Carrier.Create("Zebra Insurance", CarrierCode.Create("ZEB"));
        var carrier2 = Carrier.Create("Alpha Insurance", CarrierCode.Create("ALPHA"));
        var carrier3 = Carrier.Create("Beta Insurance", CarrierCode.Create("BETA"));

        await _repository.AddAsync(carrier1);
        await _repository.AddAsync(carrier2);
        await _repository.AddAsync(carrier3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha Insurance");
        result[1].Name.Should().Be("Beta Insurance");
        result[2].Name.Should().Be("Zebra Insurance");
    }

    [Fact]
    public async Task GetByStatusAsync_ActiveCarriers_ReturnsOnlyActive()
    {
        // Arrange
        var activeCarrier = Carrier.Create("Active Carrier", CarrierCode.Create("ACT"));
        var inactiveCarrier = Carrier.Create("Inactive Carrier", CarrierCode.Create("INA"));
        inactiveCarrier.Deactivate();

        await _repository.AddAsync(activeCarrier);
        await _repository.AddAsync(inactiveCarrier);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByStatusAsync(CarrierStatus.Active);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Carrier");
    }

    [Fact]
    public async Task ExistsByCodeAsync_ExistingCode_ReturnsTrue()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));
        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsByCodeAsync("TEST");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCodeAsync_NonExistingCode_ReturnsFalse()
    {
        // Act
        var result = await _queries.ExistsByCodeAsync("NOTEXIST");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByCodeAsync_WithExcludeId_ExcludesCarrier()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));
        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsByCodeAsync("TEST", carrier.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchByNameAsync_MatchingTerm_ReturnsMatchingCarriers()
    {
        // Arrange
        var carrier1 = Carrier.Create("Travelers Insurance", CarrierCode.Create("TRAV"));
        var carrier2 = Carrier.Create("Hartford Insurance", CarrierCode.Create("HART"));
        var carrier3 = Carrier.Create("CNA Insurance", CarrierCode.Create("CNA"));

        await _repository.AddAsync(carrier1);
        await _repository.AddAsync(carrier2);
        await _repository.AddAsync(carrier3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchByNameAsync("insurance");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchByNameAsync_PartialName_ReturnsMatchingCarriers()
    {
        // Arrange
        var carrier1 = Carrier.Create("Travelers Insurance", CarrierCode.Create("TRAV"));
        var carrier2 = Carrier.Create("Hartford Insurance", CarrierCode.Create("HART"));

        await _repository.AddAsync(carrier1);
        await _repository.AddAsync(carrier2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchByNameAsync("trav");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Travelers Insurance");
    }

    [Fact]
    public async Task ModifyCarrier_TrackedEntity_PersistsChanges()
    {
        // Arrange
        var carrier = Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));
        await _repository.AddAsync(carrier);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(carrier.Id);
        retrieved!.UpdateBasicInfo("Updated Name", "Legal Name");
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _queries.GetByIdAsync(carrier.Id);
        updated!.Name.Should().Be("Updated Name");
        updated.LegalName.Should().Be("Legal Name");
    }
}

/// <summary>
/// Test DbContext for integration tests.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Carrier> Carriers => Set<Carrier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Carrier configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Carriers.Infrastructure.Persistence.Configurations.CarrierConfiguration).Assembly);
    }
}
