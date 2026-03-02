using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Events;

namespace IBS.UnitTests.Commissions.Domain;

/// <summary>
/// Unit tests for the CommissionSchedule aggregate root.
/// </summary>
public class CommissionScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _carrierId = Guid.NewGuid();

    private CommissionSchedule CreateTestSchedule(
        decimal newBusinessRate = 15m,
        decimal renewalRate = 12m)
    {
        return CommissionSchedule.Create(
            _tenantId,
            _carrierId,
            "Acme Insurance",
            "Commercial Property",
            newBusinessRate,
            renewalRate,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));
    }

    [Fact]
    public void Create_ValidInputs_CreatesSchedule()
    {
        // Arrange & Act
        var schedule = CreateTestSchedule();

        // Assert
        schedule.Should().NotBeNull();
        schedule.TenantId.Should().Be(_tenantId);
        schedule.CarrierId.Should().Be(_carrierId);
        schedule.CarrierName.Should().Be("Acme Insurance");
        schedule.LineOfBusiness.Should().Be("Commercial Property");
        schedule.NewBusinessRate.Should().Be(15m);
        schedule.RenewalRate.Should().Be(12m);
        schedule.EffectiveFrom.Should().Be(new DateOnly(2026, 1, 1));
        schedule.EffectiveTo.Should().Be(new DateOnly(2026, 12, 31));
        schedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ValidInputs_RaisesScheduleCreatedEvent()
    {
        // Arrange & Act
        var schedule = CreateTestSchedule();

        // Assert
        schedule.DomainEvents.Should().HaveCount(1);
        var evt = schedule.DomainEvents.First().Should().BeOfType<ScheduleCreatedEvent>().Subject;
        evt.CarrierId.Should().Be(_carrierId);
        evt.CarrierName.Should().Be("Acme Insurance");
        evt.LineOfBusiness.Should().Be("Commercial Property");
    }

    [Fact]
    public void Create_NegativeRate_Throws()
    {
        // Arrange & Act
        var act = () => CommissionSchedule.Create(
            _tenantId, _carrierId, "Carrier", "LOB",
            -5m, 10m, new DateOnly(2026, 1, 1));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Commission rate*0*100*");
    }

    [Fact]
    public void Create_RateOver100_Throws()
    {
        // Arrange & Act
        var act = () => CommissionSchedule.Create(
            _tenantId, _carrierId, "Carrier", "LOB",
            15m, 105m, new DateOnly(2026, 1, 1));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Commission rate*0*100*");
    }

    [Fact]
    public void Create_EffectiveToBeforeFrom_Throws()
    {
        // Arrange & Act
        var act = () => CommissionSchedule.Create(
            _tenantId, _carrierId, "Carrier", "LOB",
            15m, 10m,
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 1, 1));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*end date*before*start date*");
    }

    [Fact]
    public void Update_ValidInputs_UpdatesSchedule()
    {
        // Arrange
        var schedule = CreateTestSchedule();

        // Act
        schedule.Update("New Carrier", "Auto", 20m, 18m, new DateOnly(2026, 6, 1));

        // Assert
        schedule.CarrierName.Should().Be("New Carrier");
        schedule.LineOfBusiness.Should().Be("Auto");
        schedule.NewBusinessRate.Should().Be(20m);
        schedule.RenewalRate.Should().Be(18m);
        schedule.EffectiveFrom.Should().Be(new DateOnly(2026, 6, 1));
        schedule.EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ActiveSchedule_DeactivatesSchedule()
    {
        // Arrange
        var schedule = CreateTestSchedule();

        // Act
        schedule.Deactivate();

        // Assert
        schedule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_Throws()
    {
        // Arrange
        var schedule = CreateTestSchedule();
        schedule.Deactivate();

        // Act
        var act = () => schedule.Deactivate();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*already inactive*");
    }
}
