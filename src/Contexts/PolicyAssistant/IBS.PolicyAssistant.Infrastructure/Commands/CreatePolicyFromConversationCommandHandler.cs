using System.Text.Json;
using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.PolicyAssistant.Application.Commands.CreatePolicyFromConversation;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Domain.Repositories;
using IBS.Policies.Application.Commands.AddCoverage;
using IBS.Policies.Application.Commands.CreatePolicy;
using IBS.Policies.Domain.ValueObjects;
using MediatR;

namespace IBS.PolicyAssistant.Infrastructure.Commands;

/// <summary>
/// Handler for the <see cref="CreatePolicyFromConversationCommand"/>.
/// Lives in Infrastructure to access Policies Application commands without polluting the Application layer.
/// Parses extracted conversation data and dispatches policy creation commands via MediatR.
/// </summary>
public sealed class CreatePolicyFromConversationCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : ICommandHandler<CreatePolicyFromConversationCommand, Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreatePolicyFromConversationCommand request, CancellationToken cancellationToken)
    {
        // Load the conversation
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            return Error.NotFound("Conversation", request.ConversationId);

        if (conversation.TenantId != request.TenantId || conversation.UserId != request.UserId)
            return Error.Forbidden();

        if (string.IsNullOrWhiteSpace(conversation.ExtractedData))
            return Error.Validation("No policy data has been extracted from this conversation yet.");

        // Parse the extracted data
        PolicyExtractionResult? extraction;
        try
        {
            extraction = JsonSerializer.Deserialize<PolicyExtractionResult>(conversation.ExtractedData, JsonOptions);
        }
        catch
        {
            return Error.Validation("Failed to parse extracted policy data.");
        }

        if (extraction is null)
            return Error.Validation("Extracted policy data is empty.");

        // Parse line of business — prefer user-confirmed value from command over extracted value
        var lobSource = request.LineOfBusiness ?? extraction.LineOfBusiness;
        if (!Enum.TryParse<LineOfBusiness>(lobSource, ignoreCase: true, out var lineOfBusiness))
            return Error.Validation($"Invalid line of business: '{lobSource}'.");

        // Parse dates — prefer user-confirmed values from command over extracted values
        var effectiveDateSource = request.EffectiveDate ?? extraction.EffectiveDate;
        if (!DateOnly.TryParse(effectiveDateSource, out var effectiveDate))
            return Error.Validation($"Invalid effective date: '{effectiveDateSource}'.");

        var expirationDateSource = request.ExpirationDate ?? extraction.ExpirationDate;
        if (!DateOnly.TryParse(expirationDateSource, out var expirationDate))
            return Error.Validation($"Invalid expiration date: '{expirationDateSource}'.");

        // Parse billing type and payment plan — prefer user-confirmed values, fall back to defaults
        var billingTypeSource = (request.BillingType ?? extraction.BillingType)?.Replace(" ", "");
        if (!Enum.TryParse<BillingType>(billingTypeSource, ignoreCase: true, out var billingType))
            billingType = BillingType.DirectBill;

        var paymentPlanSource = (request.PaymentPlan ?? extraction.PaymentPlan)?.Replace(" ", "");
        if (!Enum.TryParse<PaymentPlan>(paymentPlanSource, ignoreCase: true, out var paymentPlan))
            paymentPlan = PaymentPlan.Annual;

        // Create the policy via the Policies context command
        var createPolicyCommand = new CreatePolicyCommand(
            request.TenantId,
            request.UserId,
            request.ClientId,
            request.CarrierId,
            lineOfBusiness,
            extraction.PolicyType ?? lineOfBusiness.GetDisplayName(),
            effectiveDate,
            expirationDate,
            billingType,
            paymentPlan,
            Notes: $"Created via Policy Assistant: {conversation.Title}");

        var policyResult = await mediator.Send(createPolicyCommand, cancellationToken);
        if (!policyResult.IsSuccess)
            return policyResult.Error;

        var policyId = policyResult.Value;

        // Add coverages extracted from the conversation
        foreach (var coverage in extraction.Coverages)
        {
            if (string.IsNullOrWhiteSpace(coverage.Code) && string.IsNullOrWhiteSpace(coverage.Name))
                continue;

            _ = decimal.TryParse(coverage.Premium?.Replace("$", "").Replace(",", ""), out var premium);
            _ = decimal.TryParse(coverage.Limit?.Replace("$", "").Replace(",", ""), out var limit);
            _ = decimal.TryParse(coverage.Deductible?.Replace("$", "").Replace(",", ""), out var deductible);

            var addCoverageCommand = new AddCoverageCommand(
                request.TenantId,
                policyId,
                coverage.Code ?? "COV",
                coverage.Name ?? coverage.Code ?? "Coverage",
                premium,
                LimitAmount: limit > 0 ? limit : null,
                DeductibleAmount: deductible > 0 ? deductible : null);

            await mediator.Send(addCoverageCommand, cancellationToken);
        }

        // Mark the conversation as policy created
        conversation.MarkPolicyCreated(policyId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policyId;
    }
}
