using FluentAssertions;
using IBS.PolicyAssistant.Application.Services;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.UnitTests.PolicyAssistant;

/// <summary>
/// Unit tests for the <see cref="ChatPromptBuilder"/> static class.
/// Verifies that the correct system prompt is produced for each conversation mode
/// and that reference document context and conversation history are correctly included.
/// </summary>
public class ChatPromptBuilderTests
{
    private static readonly IReadOnlyList<DocumentSearchResult> EmptyDocs = [];
    private static readonly IReadOnlyList<ChatMessage> EmptyHistory = [];

    [Fact]
    public void Build_GuidedModeWithNoDocsOrHistory_ReturnsNonNullSystemMessage()
    {
        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, EmptyDocs, EmptyHistory);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);
        messages[0].Role.Should().Be("system");
        messages[0].Content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Build_GuidedMode_SystemPromptContainsGuidedKeywords()
    {
        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, EmptyDocs, EmptyHistory);

        // Assert
        var systemContent = messages[0].Content;
        // The guided prompt asks one question at a time
        systemContent.Should().ContainAny("question", "step", "guided", "ask");
    }

    [Fact]
    public void Build_FreeformModeWithNoDocsOrHistory_ReturnsNonNullSystemMessage()
    {
        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Freeform, EmptyDocs, EmptyHistory);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);
        messages[0].Role.Should().Be("system");
        messages[0].Content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Build_FreeformMode_SystemPromptContainsFreeformKeywords()
    {
        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Freeform, EmptyDocs, EmptyHistory);

        // Assert
        var systemContent = messages[0].Content;
        // The freeform prompt expects user to describe requirements freely
        systemContent.Should().ContainAny("describe", "freeform", "requirements", "provided");
    }

    [Fact]
    public void Build_GuidedAndFreeformMode_ProduceDifferentSystemPrompts()
    {
        // Act
        var guidedMessages = ChatPromptBuilder.Build(ConversationMode.Guided, EmptyDocs, EmptyHistory);
        var freeformMessages = ChatPromptBuilder.Build(ConversationMode.Freeform, EmptyDocs, EmptyHistory);

        // Assert
        guidedMessages[0].Content.Should().NotBe(freeformMessages[0].Content);
    }

    [Fact]
    public void Build_WithReferenceDocuments_SystemPromptContainsDocumentContent()
    {
        // Arrange
        var docs = new List<DocumentSearchResult>
        {
            new(Guid.NewGuid(), "GL Minimum Requirements", "Regulation",
                "All GL policies must carry $1,000,000 occurrence limit.", "CDOI")
        };

        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, docs, EmptyHistory);

        // Assert
        messages.Should().HaveCount(1);
        var systemContent = messages[0].Content;
        systemContent.Should().Contain("GL Minimum Requirements");
        systemContent.Should().Contain("$1,000,000 occurrence limit");
        systemContent.Should().Contain("Regulation");
    }

    [Fact]
    public void Build_WithReferenceDocumentsWithSource_SystemPromptContainsSource()
    {
        // Arrange
        var docs = new List<DocumentSearchResult>
        {
            new(Guid.NewGuid(), "Workers Comp Rules", "ValidationRule",
                "Employers must carry workers comp for any employee.", "State DOL")
        };

        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, docs, EmptyHistory);

        // Assert
        messages[0].Content.Should().Contain("State DOL");
    }

    [Fact]
    public void Build_WithConversationHistory_MessagesAreAppendedAfterSystemMessage()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new("user", "I need a general liability policy."),
            new("assistant", "Sure! What is the client name?"),
            new("user", "The client is Acme Corp.")
        };

        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, EmptyDocs, history);

        // Assert
        messages.Should().HaveCount(4); // 1 system + 3 history
        messages[0].Role.Should().Be("system");
        messages[1].Role.Should().Be("user");
        messages[1].Content.Should().Be("I need a general liability policy.");
        messages[2].Role.Should().Be("assistant");
        messages[3].Role.Should().Be("user");
        messages[3].Content.Should().Be("The client is Acme Corp.");
    }

    [Fact]
    public void Build_WithDocsAndHistory_CombinesBothInCorrectOrder()
    {
        // Arrange
        var docs = new List<DocumentSearchResult>
        {
            new(Guid.NewGuid(), "Auto Coverage Rules", "Regulation", "Minimum liability is $25,000/$50,000.", null)
        };

        var history = new List<ChatMessage>
        {
            new("user", "I need a personal auto policy."),
            new("assistant", "What is the client's name?")
        };

        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, docs, history);

        // Assert
        messages.Should().HaveCount(3); // 1 system (with doc context) + 2 history
        messages[0].Role.Should().Be("system");
        messages[0].Content.Should().Contain("Auto Coverage Rules");
        messages[0].Content.Should().Contain("$25,000/$50,000");
        messages[1].Role.Should().Be("user");
        messages[2].Role.Should().Be("assistant");
    }

    [Fact]
    public void Build_WithMultipleReferenceDocuments_AllDocumentsAppearedInSystemPrompt()
    {
        // Arrange
        var docs = new List<DocumentSearchResult>
        {
            new(Guid.NewGuid(), "GL Rules", "Regulation", "GL content here.", "Source A"),
            new(Guid.NewGuid(), "WC Rules", "ValidationRule", "WC content here.", "Source B")
        };

        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Freeform, docs, EmptyHistory);

        // Assert
        var systemContent = messages[0].Content;
        systemContent.Should().Contain("GL Rules");
        systemContent.Should().Contain("GL content here");
        systemContent.Should().Contain("WC Rules");
        systemContent.Should().Contain("WC content here");
    }

    [Fact]
    public void Build_ResultIsReadOnly_ReturnsReadOnlyList()
    {
        // Act
        var messages = ChatPromptBuilder.Build(ConversationMode.Guided, EmptyDocs, EmptyHistory);

        // Assert — result should be IReadOnlyList, not mutable
        messages.Should().BeAssignableTo<IReadOnlyList<ChatMessage>>();
    }
}
