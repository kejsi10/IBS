namespace IBS.Documents.Application.Services;

/// <summary>
/// Service interface for Azure Blob Storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to blob storage.
    /// </summary>
    /// <param name="blobKey">The unique blob key/path.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The blob key of the uploaded file.</returns>
    Task<string> UploadAsync(string blobKey, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from blob storage.
    /// </summary>
    /// <param name="blobKey">The blob key/path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the file content.</returns>
    Task<Stream> DownloadAsync(string blobKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from blob storage.
    /// </summary>
    /// <param name="blobKey">The blob key/path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(string blobKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary SAS download URL for a blob.
    /// </summary>
    /// <param name="blobKey">The blob key/path.</param>
    /// <param name="expiry">How long the URL should remain valid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A temporary download URL.</returns>
    Task<string> GetTemporaryDownloadUrlAsync(string blobKey, TimeSpan expiry, CancellationToken cancellationToken = default);
}
