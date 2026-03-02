using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Enums;
using IBS.PolicyAssistant.Domain.Events;

namespace IBS.PolicyAssistant.Domain.Aggregates.Conversation;

/// <summary>
/// Aggregate root representing a policy assistant chat conversation.
/// Tracks the conversation history, extracted policy data, and validation results.
/// </summary>
public sealed class Conversation : TenantAggregateRoot
{
    private readonly List<ChatMessageEntity> _messages = [];

    /// <summary>
    /// Gets the identifier of the user who owns this conversation.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the display title of the conversation.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the conversation mode (Guided or Freeform).
    /// </summary>
    public ConversationMode Mode { get; private set; }

    /// <summary>
    /// Gets the current status of the conversation.
    /// </summary>
    public ConversationStatus Status { get; private set; }

    /// <summary>
    /// Gets the line of business being discussed (e.g., PersonalAuto, GeneralLiability).
    /// </summary>
    public string? LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the policy identifier if a policy was created from this conversation.
    /// </summary>
    public Guid? PolicyId { get; private set; }

    /// <summary>
    /// Gets the JSON-serialized extracted policy data, updated after each message.
    /// </summary>
    public string? ExtractedData { get; private set; }

    /// <summary>
    /// Gets the messages in this conversation.
    /// </summary>
    public IReadOnlyCollection<ChatMessageEntity> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Conversation() { }

    /// <summary>
    /// Creates a new policy assistant conversation.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="title">The conversation title.</param>
    /// <param name="mode">The conversation mode.</param>
    /// <param name="lineOfBusiness">Optional initial line of business.</param>
    /// <returns>A new <see cref="Conversation"/>.</returns>
    public static Conversation Create(
        Guid tenantId,
        Guid userId,
        string title,
        ConversationMode mode,
        string? lineOfBusiness = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var conversation = new Conversation
        {
            TenantId = tenantId,
            UserId = userId,
            Title = title.Trim(),
            Mode = mode,
            Status = ConversationStatus.Active,
            LineOfBusiness = lineOfBusiness
        };

        conversation.RaiseDomainEvent(new ConversationCreatedEvent(
            conversation.Id,
            conversation.TenantId,
            conversation.UserId,
            conversation.Mode));

        return conversation;
    }

    /// <summary>
    /// Adds a message to the conversation.
    /// </summary>
    /// <param name="role">The sender role (user, assistant, system).</param>
    /// <param name="content">The message content.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="metadata">Optional JSON metadata.</param>
    public void AddMessage(string role, string content, MessageType messageType = MessageType.Chat, string? metadata = null)
    {
        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException("Cannot add messages to an inactive conversation.");

        var message = ChatMessageEntity.Create(Id, role, content, messageType, metadata);
        _messages.Add(message);
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the extracted policy data from the AI extraction result.
    /// </summary>
    /// <param name="extractedDataJson">The JSON-serialized extracted policy data.</param>
    /// <param name="lineOfBusiness">The detected line of business.</param>
    public void UpdateExtractedData(string extractedDataJson, string? lineOfBusiness = null)
    {
        ExtractedData = extractedDataJson;
        if (!string.IsNullOrWhiteSpace(lineOfBusiness))
            LineOfBusiness = lineOfBusiness;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the conversation as having created a policy.
    /// </summary>
    /// <param name="policyId">The created policy identifier.</param>
    public void MarkPolicyCreated(Guid policyId)
    {
        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException("Only active conversations can have a policy created.");

        PolicyId = policyId;
        Status = ConversationStatus.PolicyCreated;
        MarkAsUpdated();
        RaiseDomainEvent(new ConversationPolicyCreatedEvent(Id, TenantId, policyId));
    }

    /// <summary>
    /// Marks the conversation as abandoned.
    /// </summary>
    public void Abandon()
    {
        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException("Only active conversations can be abandoned.");

        Status = ConversationStatus.Abandoned;
        MarkAsUpdated();
    }
}
