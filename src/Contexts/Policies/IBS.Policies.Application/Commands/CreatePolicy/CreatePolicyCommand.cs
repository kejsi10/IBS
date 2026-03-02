using IBS.BuildingBlocks.Application.Commands;
using IBS.Carriers.Domain.ValueObjects;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.CreatePolicy;

/// <summary>
/// Command to create a new policy.
/// </summary>
public sealed record CreatePolicyCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClientId,
    Guid CarrierId,
    LineOfBusiness LineOfBusiness,
    string PolicyType,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    BillingType BillingType = BillingType.DirectBill,
    PaymentPlan PaymentPlan = PaymentPlan.Annual,
    Guid? QuoteId = null,
    string? Notes = null
) : ICommand<Guid>;
