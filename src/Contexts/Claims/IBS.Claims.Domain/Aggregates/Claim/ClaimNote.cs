using IBS.BuildingBlocks.Domain;

namespace IBS.Claims.Domain.Aggregates.Claim;

/// <summary>
/// Represents a note attached to a claim.
/// </summary>
public sealed class ClaimNote : Entity
{
    /// <summary>
    /// Gets the claim identifier.
    /// </summary>
    public Guid ClaimId { get; private set; }

    /// <summary>
    /// Gets the note content.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the user who authored the note.
    /// </summary>
    public string AuthorBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this note is internal (not visible to the insured).
    /// </summary>
    public bool IsInternal { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private ClaimNote() { }

    /// <summary>
    /// Creates a new claim note.
    /// </summary>
    /// <param name="claimId">The claim identifier.</param>
    /// <param name="content">The note content.</param>
    /// <param name="authorBy">The author identifier.</param>
    /// <param name="isInternal">Whether the note is internal.</param>
    /// <returns>A new ClaimNote instance.</returns>
    public static ClaimNote Create(Guid claimId, string content, string authorBy, bool isInternal = false)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content is required.", nameof(content));

        if (string.IsNullOrWhiteSpace(authorBy))
            throw new ArgumentException("Author is required.", nameof(authorBy));

        return new ClaimNote
        {
            ClaimId = claimId,
            Content = content.Trim(),
            AuthorBy = authorBy.Trim(),
            IsInternal = isInternal
        };
    }
}
