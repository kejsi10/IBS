using FluentAssertions;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.ValueObjects;
using IBS.Clients.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Clients;

/// <summary>
/// Integration tests for ClientRepository using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class ClientRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private ClientTestDbContext _context = null!;
    private ClientRepository _repository = null!;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ClientRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClientTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"ClientTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new ClientTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new ClientRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_IndividualClient_PersistsClient()
    {
        // Arrange
        var client = Client.CreateIndividual(
            _tenantId,
            PersonName.Create("John", "Doe"),
            _userId,
            email: EmailAddress.Create("john@test.com"));

        // Act
        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(client.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ClientType.Should().Be(ClientType.Individual);
        retrieved.PersonName!.FirstName.Should().Be("John");
        retrieved.PersonName.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task AddAsync_BusinessClient_PersistsClient()
    {
        // Arrange
        var client = Client.CreateBusiness(
            _tenantId,
            BusinessInfo.Create("Acme Corp", "Corporation", industry: "Technology"),
            _userId,
            email: EmailAddress.Create("info@acme.com"));

        // Act
        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(client.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ClientType.Should().Be(ClientType.Business);
        retrieved.BusinessInfo!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingClient_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithChildrenAsync_ClientWithContacts_ReturnsContacts()
    {
        // Arrange
        var client = Client.CreateBusiness(
            _tenantId,
            BusinessInfo.Create("Test Corp", "LLC"),
            _userId);
        client.AddContact(
            PersonName.Create("Jane", "Smith"),
            title: "CEO",
            email: EmailAddress.Create("jane@test.com"),
            isPrimary: true);
        client.AddContact(
            PersonName.Create("Bob", "Jones"),
            title: "CFO");

        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithChildrenAsync(client.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Contacts.Should().HaveCount(2);
        retrieved.Contacts.Should().Contain(c => c.Name.FirstName == "Jane" && c.IsPrimary);
    }

    [Fact]
    public async Task GetByIdWithChildrenAsync_ClientWithAddresses_ReturnsAddresses()
    {
        // Arrange
        var client = Client.CreateIndividual(
            _tenantId,
            PersonName.Create("John", "Doe"),
            _userId);
        client.AddAddress(
            AddressType.Physical,
            "123 Main St",
            "Springfield",
            "IL",
            "62701",
            isPrimary: true);
        client.AddAddress(
            AddressType.Mailing,
            "PO Box 100",
            "Springfield",
            "IL",
            "62701");

        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithChildrenAsync(client.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Addresses.Should().HaveCount(2);
        retrieved.Addresses.Should().Contain(a => a.AddressType == AddressType.Physical && a.IsPrimary);
    }

    [Fact]
    public async Task GetByIdWithChildrenAsync_ClientWithCommunications_ReturnsCommunications()
    {
        // Arrange
        var client = Client.CreateIndividual(
            _tenantId,
            PersonName.Create("John", "Doe"),
            _userId);
        client.LogCommunication(
            CommunicationType.Email,
            "Welcome Email",
            "Sent welcome email to client",
            _userId);
        client.LogCommunication(
            CommunicationType.Phone,
            "Follow-up Call",
            null,
            _userId);

        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithChildrenAsync(client.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Communications.Should().HaveCount(2);
    }

    [Fact]
    public async Task ModifyClient_Deactivate_PersistsStatus()
    {
        // Arrange
        var client = Client.CreateIndividual(
            _tenantId,
            PersonName.Create("John", "Doe"),
            _userId);
        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(client.Id);
        retrieved!.Deactivate();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var deactivated = await _repository.GetByIdAsync(client.Id);
        deactivated!.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public async Task ModifyClient_AddAndRemoveContact_PersistsChanges()
    {
        // Arrange
        var client = Client.CreateBusiness(
            _tenantId,
            BusinessInfo.Create("Test Corp", "LLC"),
            _userId);
        var contact = client.AddContact(
            PersonName.Create("Jane", "Smith"),
            isPrimary: true);

        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - remove contact
        var retrieved = await _repository.GetByIdWithChildrenAsync(client.Id);
        retrieved!.RemoveContact(contact.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdWithChildrenAsync(client.Id);
        updated!.Contacts.Should().BeEmpty();
    }

    [Fact]
    public async Task ModifyClient_SetPrimaryContact_PersistsChanges()
    {
        // Arrange
        var client = Client.CreateBusiness(
            _tenantId,
            BusinessInfo.Create("Test Corp", "LLC"),
            _userId);
        var contact1 = client.AddContact(
            PersonName.Create("Jane", "Smith"),
            isPrimary: true);
        var contact2 = client.AddContact(
            PersonName.Create("Bob", "Jones"));

        await _repository.AddAsync(client);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithChildrenAsync(client.Id);
        retrieved!.SetPrimaryContact(contact2.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdWithChildrenAsync(client.Id);
        updated!.Contacts.First(c => c.Id == contact2.Id).IsPrimary.Should().BeTrue();
        updated.Contacts.First(c => c.Id == contact1.Id).IsPrimary.Should().BeFalse();
    }
}

/// <summary>
/// Test DbContext for Client integration tests.
/// </summary>
public class ClientTestDbContext : DbContext
{
    public ClientTestDbContext(DbContextOptions<ClientTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the clients set.
    /// </summary>
    public DbSet<Client> Clients => Set<Client>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Client configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Clients.Infrastructure.Persistence.Configurations.ClientConfiguration).Assembly);
    }
}
