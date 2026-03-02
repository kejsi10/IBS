using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.Events;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.UnitTests.Tenants.Domain;

/// <summary>
/// Unit tests for the Tenant aggregate root.
/// </summary>
public class TenantTests
{
    [Fact]
    public void Create_ValidInputs_CreatesTenant()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var tenant = Tenant.Create("Acme Insurance", subdomain, SubscriptionTier.Professional);

        // Assert
        tenant.Should().NotBeNull();
        tenant.Name.Should().Be("Acme Insurance");
        tenant.Subdomain.Should().Be(subdomain);
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.SubscriptionTier.Should().Be(SubscriptionTier.Professional);
        tenant.Settings.Should().BeNull();
        tenant.Carriers.Should().BeEmpty();
    }

    [Fact]
    public void Create_RaisesTenantRegisteredEvent()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var tenant = Tenant.Create("Acme Insurance", subdomain, SubscriptionTier.Basic);

        // Assert
        tenant.DomainEvents.Should().HaveCount(1);
        var domainEvent = tenant.DomainEvents.First().Should().BeOfType<TenantRegisteredEvent>().Subject;
        domainEvent.TenantId.Should().Be(tenant.Id);
        domainEvent.Name.Should().Be("Acme Insurance");
        domainEvent.Subdomain.Should().Be("acme");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var tenant = Tenant.Create("  Acme Insurance  ", subdomain, SubscriptionTier.Basic);

        // Assert
        tenant.Name.Should().Be("Acme Insurance");
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var act = () => Tenant.Create("", subdomain, SubscriptionTier.Basic);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Create_WhitespaceName_ThrowsException()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var act = () => Tenant.Create("   ", subdomain, SubscriptionTier.Basic);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        // Arrange
        var tenant = CreateTestTenant();

        // Act
        tenant.UpdateName("New Company Name");

        // Assert
        tenant.Name.Should().Be("New Company Name");
    }

    [Fact]
    public void UpdateName_TrimsWhitespace()
    {
        // Arrange
        var tenant = CreateTestTenant();

        // Act
        tenant.UpdateName("  New Company Name  ");

        // Assert
        tenant.Name.Should().Be("New Company Name");
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsException()
    {
        // Arrange
        var tenant = CreateTestTenant();

        // Act
        var act = () => tenant.UpdateName("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Suspend_ActiveTenant_SuspendsTenant()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.ClearDomainEvents();

        // Act
        tenant.Suspend();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.DomainEvents.Should().HaveCount(1);
        tenant.DomainEvents.First().Should().BeOfType<TenantSuspendedEvent>();
    }

    [Fact]
    public void Suspend_CancelledTenant_ThrowsException()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.Cancel();

        // Act
        var act = () => tenant.Suspend();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*cancelled*");
    }

    [Fact]
    public void Activate_SuspendedTenant_ActivatesTenant()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.Suspend();
        tenant.ClearDomainEvents();

        // Act
        tenant.Activate();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.DomainEvents.Should().HaveCount(1);
        tenant.DomainEvents.First().Should().BeOfType<TenantActivatedEvent>();
    }

    [Fact]
    public void Activate_CancelledTenant_ThrowsException()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.Cancel();

        // Act
        var act = () => tenant.Activate();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*cancelled*");
    }

    [Fact]
    public void Cancel_ActiveTenant_CancelsTenant()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.ClearDomainEvents();

        // Act
        tenant.Cancel();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Cancelled);
        tenant.DomainEvents.Should().HaveCount(1);
        tenant.DomainEvents.First().Should().BeOfType<TenantCancelledEvent>();
    }

    [Fact]
    public void Cancel_SuspendedTenant_CancelsTenant()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.Suspend();
        tenant.ClearDomainEvents();

        // Act
        tenant.Cancel();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Cancelled);
        tenant.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateSubscriptionTier_UpdatesTier()
    {
        // Arrange
        var tenant = CreateTestTenant();

        // Act
        tenant.UpdateSubscriptionTier(SubscriptionTier.Enterprise);

        // Assert
        tenant.SubscriptionTier.Should().Be(SubscriptionTier.Enterprise);
    }

    [Fact]
    public void UpdateSettings_UpdatesSettings()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var settings = """{"theme":"dark"}""";

        // Act
        tenant.UpdateSettings(settings);

        // Assert
        tenant.Settings.Should().Be(settings);
    }

    [Fact]
    public void UpdateSettings_NullSettings_ClearsSettings()
    {
        // Arrange
        var tenant = CreateTestTenant();
        tenant.UpdateSettings("""{"theme":"dark"}""");

        // Act
        tenant.UpdateSettings(null);

        // Assert
        tenant.Settings.Should().BeNull();
    }

    [Fact]
    public void AddCarrier_ValidCarrier_AddsCarrier()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();

        // Act
        tenant.AddCarrier(carrierId, "AGENCY123", 0.15m);

        // Assert
        tenant.Carriers.Should().HaveCount(1);
        var carrier = tenant.Carriers.First();
        carrier.CarrierId.Should().Be(carrierId);
        carrier.AgencyCode.Should().Be("AGENCY123");
        carrier.CommissionRate.Should().Be(0.15m);
        carrier.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddCarrier_WithoutOptionalFields_AddsCarrier()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();

        // Act
        tenant.AddCarrier(carrierId);

        // Assert
        tenant.Carriers.Should().HaveCount(1);
        var carrier = tenant.Carriers.First();
        carrier.CarrierId.Should().Be(carrierId);
        carrier.AgencyCode.Should().BeNull();
        carrier.CommissionRate.Should().BeNull();
    }

    [Fact]
    public void AddCarrier_SameCarrierTwice_ThrowsException()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId);

        // Act
        var act = () => tenant.AddCarrier(carrierId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*already associated*");
    }

    [Fact]
    public void RemoveCarrier_ExistingCarrier_RemovesCarrier()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId);

        // Act
        tenant.RemoveCarrier(carrierId);

        // Assert
        tenant.Carriers.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCarrier_NonExistingCarrier_ThrowsException()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();

        // Act
        var act = () => tenant.RemoveCarrier(carrierId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*not associated*");
    }

    [Fact]
    public void AddMultipleCarriers_AddsAllCarriers()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId1 = Guid.NewGuid();
        var carrierId2 = Guid.NewGuid();
        var carrierId3 = Guid.NewGuid();

        // Act
        tenant.AddCarrier(carrierId1);
        tenant.AddCarrier(carrierId2);
        tenant.AddCarrier(carrierId3);

        // Assert
        tenant.Carriers.Should().HaveCount(3);
    }

    private static Tenant CreateTestTenant()
    {
        var subdomain = Subdomain.Create("acme");
        return Tenant.Create("Acme Insurance", subdomain, SubscriptionTier.Professional);
    }
}
