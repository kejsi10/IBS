namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Marker interface for DTOs that carry a concurrency token (row version).
/// Implementations should return the Base64-encoded row version.
/// </summary>
public interface IConcurrencyAware
{
    /// <summary>
    /// Gets the Base64-encoded row version for concurrency control.
    /// </summary>
    string RowVersion { get; }
}
