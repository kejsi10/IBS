using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Commands.ReinstatePolicy;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Events;
using IBS.Policies.Domain.Repositories;
using IBS.Policies.Domain.ValueObjects;
using NSubstitute;

namespace IBS.UnitTests.Policies.Application;

/// <summary>
/// Unit tests for the ReinstatePolicyCommandHandler.
/// </summary>
public class ReinstatePolicyCommandHandlerTests
{
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReinstatePolicyCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReinstatePolicyCommandHandlerTests"/> class.
    /// </summary>
    public ReinstatePolicyCommandHandlerTests()
    {
        _handler = new ReinstatePolicyCommandHandler(_policyRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_CancelledPolicy_ReinstatesPolicy()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.InsuredRequest);
        var command = new ReinstatePolicyCommand(_tenantId, policy.Id, "Customer resolved billing issue");
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        policy.Status.Should().Be(PolicyStatus.Active);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PolicyNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var command = new ReinstatePolicyCommand(_tenantId, policyId, "Some reason");
        _policyRepository.GetByIdAsync(policyId, Arg.Any<CancellationToken>()).Returns((Policy?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_FlatCancelledPolicy_ReturnsValidationError()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.FlatCancel);
        var command = new ReinstatePolicyCommand(_tenantId, policy.Id, "Trying to reinstate");
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MisrepresentationPolicy_ReturnsValidationError()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.Misrepresentation);
        var command = new ReinstatePolicyCommand(_tenantId, policy.Id, "Trying to reinstate");
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Policy CreateCancelledPolicy(CancellationType cancellationType)
    {
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var policy = Policy.Create(
            _tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId);

        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();
        var cancellationDate = effectivePeriod.EffectiveDate.AddMonths(1);
        policy.Cancel(cancellationDate, "Test cancellation", cancellationType);
        return policy;
    }
}
