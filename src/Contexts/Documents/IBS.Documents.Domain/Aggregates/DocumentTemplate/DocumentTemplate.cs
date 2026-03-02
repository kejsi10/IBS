using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Events;

namespace IBS.Documents.Domain.Aggregates.DocumentTemplate;

/// <summary>
/// Represents a Handlebars document template stored in the database.
/// </summary>
public sealed class DocumentTemplate : TenantAggregateRoot
{
    /// <summary>
    /// Gets the template name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the template description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the template type.
    /// </summary>
    public TemplateType TemplateType { get; private set; }

    /// <summary>
    /// Gets the Handlebars HTML content.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the template is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the version number.
    /// </summary>
    public int Version { get; private set; } = 1;

    /// <summary>
    /// Gets the identifier of the user who created the template.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    private DocumentTemplate() { }

    /// <summary>
    /// Creates a new document template.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="name">The template name.</param>
    /// <param name="description">The template description.</param>
    /// <param name="templateType">The template type.</param>
    /// <param name="content">The Handlebars HTML content.</param>
    /// <param name="createdBy">The user who creates the template.</param>
    /// <returns>The new document template.</returns>
    public static DocumentTemplate Create(
        Guid tenantId,
        string name,
        string description,
        TemplateType templateType,
        string content,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));

        var template = new DocumentTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description ?? string.Empty,
            TemplateType = templateType,
            Content = content,
            IsActive = false,
            Version = 1,
            CreatedBy = createdBy
        };

        template.RaiseDomainEvent(new DocumentTemplateCreatedEvent(
            template.Id,
            tenantId,
            name,
            templateType));

        return template;
    }

    /// <summary>
    /// Updates the template content. Bumps the version number.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="content">The new Handlebars content.</param>
    public void Update(string name, string description, string content)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Name = name;
        Description = description ?? string.Empty;
        Content = content;
        Version++;

        RaiseDomainEvent(new DocumentTemplateUpdatedEvent(Id, TenantId, name, Version));
    }

    /// <summary>
    /// Activates the template.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Template is already active.");

        IsActive = true;
    }

    /// <summary>
    /// Deactivates the template.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Template is already inactive.");

        IsActive = false;
    }
}
