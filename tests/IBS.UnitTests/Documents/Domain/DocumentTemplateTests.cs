using FluentAssertions;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Events;

namespace IBS.UnitTests.Documents.Domain;

/// <summary>
/// Unit tests for the DocumentTemplate aggregate.
/// </summary>
public class DocumentTemplateTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private const string Name = "Standard COI Template";
    private const string Description = "Default COI template";
    private const string Content = "<h1>COI - {{PolicyNumber}}</h1><p>Insured: {{ClientName}}</p>";
    private const string CreatedBy = "admin-user";

    [Fact]
    public void Create_WithValidInputs_CreatesTemplate()
    {
        // Arrange & Act
        var template = DocumentTemplate.Create(
            TenantId,
            Name,
            Description,
            TemplateType.CertificateOfInsurance,
            Content,
            CreatedBy);

        // Assert
        template.Id.Should().NotBeEmpty();
        template.TenantId.Should().Be(TenantId);
        template.Name.Should().Be(Name);
        template.Description.Should().Be(Description);
        template.TemplateType.Should().Be(TemplateType.CertificateOfInsurance);
        template.Content.Should().Be(Content);
        template.IsActive.Should().BeFalse();
        template.Version.Should().Be(1);
        template.CreatedBy.Should().Be(CreatedBy);
    }

    [Fact]
    public void Create_RaisesDocumentTemplateCreatedEvent()
    {
        // Arrange & Act
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Assert
        template.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentTemplateCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DocumentTemplate.Create(TenantId, string.Empty, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, string.Empty, CreatedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("content");
    }

    [Fact]
    public void Update_ChangesPropertiesAndBumpsVersion()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);
        template.ClearDomainEvents();

        // Act
        template.Update("Updated Name", "Updated Desc", "<h1>New Content</h1>");

        // Assert
        template.Name.Should().Be("Updated Name");
        template.Description.Should().Be("Updated Desc");
        template.Content.Should().Be("<h1>New Content</h1>");
        template.Version.Should().Be(2);
    }

    [Fact]
    public void Update_RaisesDocumentTemplateUpdatedEvent()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);
        template.ClearDomainEvents();

        // Act
        template.Update("Updated Name", "Updated Desc", "<h1>New Content</h1>");

        // Assert
        template.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentTemplateUpdatedEvent>();
    }

    [Fact]
    public void Update_MultipleTimesIncrementsVersion()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Act
        template.Update("v2", "", "<p>v2</p>");
        template.Update("v3", "", "<p>v3</p>");

        // Assert
        template.Version.Should().Be(3);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Act
        var act = () => template.Update(string.Empty, Description, Content);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Activate_WhenInactive_SetsIsActiveTrue()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Act
        template.Activate();

        // Assert
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);
        template.Activate();

        // Act
        var act = () => template.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveFalse()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);
        template.Activate();

        // Act
        template.Deactivate();

        // Assert
        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        var template = DocumentTemplate.Create(TenantId, Name, Description,
            TemplateType.CertificateOfInsurance, Content, CreatedBy);

        // Act
        var act = () => template.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already inactive*");
    }
}
