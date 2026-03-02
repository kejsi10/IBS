using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using IBS.Documents.Application.Services;
using Microsoft.Extensions.Configuration;

namespace IBS.Documents.Infrastructure.Storage;

/// <summary>
/// Azure Blob Storage implementation of IBlobStorageService.
/// </summary>
public sealed class AzureBlobStorageService : IBlobStorageService
{
    private const string ContainerName = "ibs-documents";
    private readonly BlobServiceClient _serviceClient;

    /// <summary>
    /// Initializes a new instance of the AzureBlobStorageService class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
        _serviceClient = new BlobServiceClient(connectionString);
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(string blobKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobKey);

        await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return blobKey;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAsync(string blobKey, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobKey);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string blobKey, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobKey);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetTemporaryDownloadUrlAsync(string blobKey, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobKey);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = ContainerName,
                BlobName = blobKey,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        // Fallback for Azurite or environments without SAS support
        return blobClient.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerClient = _serviceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }
}
