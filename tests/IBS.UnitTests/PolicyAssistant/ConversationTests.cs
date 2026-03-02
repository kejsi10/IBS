using FluentAssertions;
using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using IBS.PolicyAssistant.Domain.Enums;
using IBS.PolicyAssistant.Domain.Events;

namespace IBS.UnitTests.PolicyAssistant;

/// <summary>
/// Unit tests for the <see cref="Conversation"/> domain aggregate.
/// </summary>
public class ConversationTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private Conversation CreateActiveConversation(
        ConversationMode mode = ConversationMode.Guided,
        string? lineOfBusiness = null)
    {
        return Conversation.Create(
            _tenantId,
            _userId,
            "Test Conversation",
            mode,
            lineOfBusiness);
    }

    [Fact]
    public void Create_ValidInputs_CreatesConversationWithActiveStatus()
    {
        // Arrange & Act
        var conversation = Conversation.Create(
            _tenantId,
            _userId,
            "New Policy Conversation",
            ConversationMode.Guided,
            "GeneralLiability");

        // Assert
        conversation.Should().NotBeNull();
        conversation.Id.Should().NotBeEmpty();
        conversation.TenantId.Should().Be(_tenantId);
        conversation.UserId.Should().Be(_userId);
        conversation.Title.Should().Be("New Policy Conversation");
        conversation.Mode.Should().Be(ConversationMode.Guided);
        conversation.Status.Should().Be(ConversationStatus.Active);
        conversation.LineOfBusiness.Should().Be("GeneralLiability");
        conversation.PolicyId.Should().BeNull();
        conversation.ExtractedData.Should().BeNull();
        conversation.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithFreeformMode_SetsFreeformMode()
    {
        // Arrange & Act
        var conversation = CreateActiveConversation(ConversationMode.Freeform);

        // Assert
        conversation.Mode.Should().Be(ConversationMode.Freeform);
    }

    [Fact]
    public void Create_WithNoLineOfBusiness_LineOfBusinessIsNull()
    {
        // Arrange & Act
        var conversation = Conversation.Create(_tenantId, _userId, "Test", ConversationMode.Guided);

        // Assert
        conversation.LineOfBusiness.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesConversationCreatedEvent()
    {
        // Arrange & Act
        var conversation = CreateActiveConversation();

        // Assert
        conversation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ConversationCreatedEvent>();

        var evt = (ConversationCreatedEvent)conversation.DomainEvents.First();
        evt.ConversationId.Should().Be(conversation.Id);
        evt.TenantId.Should().Be(_tenantId);
        evt.UserId.Should().Be(_userId);
        evt.Mode.Should().Be(ConversationMode.Guided);
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ThrowsArgumentException()
    {
        // Act
        var act = () => Conversation.Create(_tenantId, _userId, "   ", ConversationMode.Guided);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_TitleIsTrimmeed_TrimsTitleWhitespace()
    {
        // Arrange & Act
        var conversation = Conversation.Create(_tenantId, _userId, "  My Title  ", ConversationMode.Guided);

        // Assert
        conversation.Title.Should().Be("My Title");
    }

    [Fact]
    public void AddMessage_ActiveConversation_AppendsMessageAndUpdatesUpdatedAt()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        var beforeAdd = conversation.UpdatedAt;

        // Act
        conversation.AddMessage("user", "I need a general liability policy.");

        // Assert
        conversation.Messages.Should().HaveCount(1);
        var message = conversation.Messages.First();
        message.Role.Should().Be("user");
        message.Content.Should().Be("I need a general liability policy.");
        message.MessageType.Should().Be(MessageType.Chat);
        conversation.UpdatedAt.Should().BeOnOrAfter(beforeAdd);
    }

    [Fact]
    public void AddMessage_MultipleMessages_AppendsInOrder()
    {
        // Arrange
        var conversation = CreateActiveConversation();

        // Act
        conversation.AddMessage("user", "First message.");
        conversation.AddMessage("assistant", "Second message.");
        conversation.AddMessage("user", "Third message.");

        // Assert
        conversation.Messages.Should().HaveCount(3);
        conversation.Messages.ElementAt(0).Role.Should().Be("user");
        conversation.Messages.ElementAt(1).Role.Should().Be("assistant");
        conversation.Messages.ElementAt(2).Role.Should().Be("user");
    }

    [Fact]
    public void AddMessage_WithMessageType_SetsMessageType()
    {
        // Arrange
        var conversation = CreateActiveConversation();

        // Act
        conversation.AddMessage("assistant", "{}", MessageType.PolicyExtraction);

        // Assert
        conversation.Messages.First().MessageType.Should().Be(MessageType.PolicyExtraction);
    }

    [Fact]
    public void AddMessage_WhenAbandoned_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.Abandon();

        // Act
        var act = () => conversation.AddMessage("user", "Hello");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void AddMessage_WhenPolicyCreated_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.MarkPolicyCreated(Guid.NewGuid());

        // Act
        var act = () => conversation.AddMessage("user", "Hello");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void UpdateExtractedData_SetsExtractedDataJson()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        const string json = """{"clientName":"Acme Corp","isComplete":false}""";

        // Act
        conversation.UpdateExtractedData(json);

        // Assert
        conversation.ExtractedData.Should().Be(json);
    }

    [Fact]
    public void UpdateExtractedData_WithLineOfBusiness_UpdatesLineOfBusiness()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        const string json = """{"isComplete":false}""";

        // Act
        conversation.UpdateExtractedData(json, "PersonalAuto");

        // Assert
        conversation.LineOfBusiness.Should().Be("PersonalAuto");
    }

    [Fact]
    public void UpdateExtractedData_WithNullLineOfBusiness_DoesNotOverwriteExistingLob()
    {
        // Arrange
        var conversation = CreateActiveConversation(lineOfBusiness: "GeneralLiability");

        // Act
        conversation.UpdateExtractedData("{}", null);

        // Assert
        conversation.LineOfBusiness.Should().Be("GeneralLiability");
    }

    [Fact]
    public void MarkPolicyCreated_ActiveConversation_SetsPolicyCreatedStatusAndPolicyId()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        var policyId = Guid.NewGuid();

        // Act
        conversation.MarkPolicyCreated(policyId);

        // Assert
        conversation.Status.Should().Be(ConversationStatus.PolicyCreated);
        conversation.PolicyId.Should().Be(policyId);
    }

    [Fact]
    public void MarkPolicyCreated_RaisesConversationPolicyCreatedEvent()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.ClearDomainEvents();
        var policyId = Guid.NewGuid();

        // Act
        conversation.MarkPolicyCreated(policyId);

        // Assert
        conversation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ConversationPolicyCreatedEvent>();

        var evt = (ConversationPolicyCreatedEvent)conversation.DomainEvents.First();
        evt.ConversationId.Should().Be(conversation.Id);
        evt.TenantId.Should().Be(_tenantId);
        evt.PolicyId.Should().Be(policyId);
    }

    [Fact]
    public void MarkPolicyCreated_WhenAbandoned_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.Abandon();

        // Act
        var act = () => conversation.MarkPolicyCreated(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Fact]
    public void Abandon_ActiveConversation_SetsStatusToAbandoned()
    {
        // Arrange
        var conversation = CreateActiveConversation();

        // Act
        conversation.Abandon();

        // Assert
        conversation.Status.Should().Be(ConversationStatus.Abandoned);
    }

    [Fact]
    public void Abandon_WhenPolicyCreated_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.MarkPolicyCreated(Guid.NewGuid());

        // Act
        var act = () => conversation.Abandon();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Fact]
    public void Abandon_WhenAlreadyAbandoned_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversation = CreateActiveConversation();
        conversation.Abandon();

        // Act
        var act = () => conversation.Abandon();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }
}
