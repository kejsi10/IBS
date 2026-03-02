namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Helper for validating ETag/If-Match concurrency tokens.
/// </summary>
public static class ConcurrencyGuard
{
    /// <summary>
    /// Validates that the expected row version matches the current row version.
    /// If no expected version is provided, the check is skipped (no If-Match header).
    /// </summary>
    /// <param name="expectedRowVersion">The Base64-encoded row version from the If-Match header, or null if not provided.</param>
    /// <param name="currentRowVersion">The current row version from the entity.</param>
    /// <exception cref="ConcurrencyConflictException">Thrown when the versions do not match.</exception>
    public static void Validate(string? expectedRowVersion, byte[] currentRowVersion)
    {
        if (expectedRowVersion is null)
            return;

        var currentBase64 = Convert.ToBase64String(currentRowVersion);

        if (!string.Equals(expectedRowVersion, currentBase64, StringComparison.Ordinal))
        {
            throw new ConcurrencyConflictException(currentBase64);
        }
    }
}
