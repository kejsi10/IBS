using FluentAssertions;
using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.UnitTests.Tenants.Domain;

/// <summary>
/// Unit tests for the TenantCarrier entity.
/// </summary>
public class TenantCarrierTests
{
    [Fact]
    public void AddCarrierToTenant_CreatesActiveRelationship()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();

        // Act
        tenant.AddCarrier(carrierId, "AGENCY123", 0.15m);

        // Assert
        var carrier = tenant.Carriers.First();
        carrier.TenantId.Should().Be(tenant.Id);
        carrier.CarrierId.Should().Be(carrierId);
        carrier.AgencyCode.Should().Be("AGENCY123");
        carrier.CommissionRate.Should().Be(0.15m);
        carrier.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateAgencyCode_UpdatesAgencyCode()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "OLD_CODE", 0.10m);
        var carrier = tenant.Carriers.First();

        // Act
        carrier.UpdateAgencyCode("NEW_CODE");

        // Assert
        carrier.AgencyCode.Should().Be("NEW_CODE");
    }

    [Fact]
    public void UpdateAgencyCode_ToNull_ClearsAgencyCode()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "OLD_CODE", 0.10m);
        var carrier = tenant.Carriers.First();

        // Act
        carrier.UpdateAgencyCode(null);

        // Assert
        carrier.AgencyCode.Should().BeNull();
    }

    [Fact]
    public void UpdateCommissionRate_UpdatesRate()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "AGENCY", 0.10m);
        var carrier = tenant.Carriers.First();

        // Act
        carrier.UpdateCommissionRate(0.20m);

        // Assert
        carrier.CommissionRate.Should().Be(0.20m);
    }

    [Fact]
    public void UpdateCommissionRate_ToNull_ClearsRate()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "AGENCY", 0.10m);
        var carrier = tenant.Carriers.First();

        // Act
        carrier.UpdateCommissionRate(null);

        // Assert
        carrier.CommissionRate.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ActiveCarrier_DeactivatesCarrier()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "AGENCY", 0.10m);
        var carrier = tenant.Carriers.First();

        // Act
        carrier.Deactivate();

        // Assert
        carrier.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_InactiveCarrier_ActivatesCarrier()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "AGENCY", 0.10m);
        var carrier = tenant.Carriers.First();
        carrier.Deactivate();

        // Act
        carrier.Activate();

        // Assert
        carrier.IsActive.Should().BeTrue();
    }

    private static Tenant CreateTestTenant()
    {
        var subdomain = Subdomain.Create("acme");
        return Tenant.Create("Acme Insurance", subdomain, SubscriptionTier.Professional);
    }
}
