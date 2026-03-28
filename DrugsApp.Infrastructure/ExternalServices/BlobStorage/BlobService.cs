using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DrugsApp.Domain.Common;

namespace DrugsApp.Infrastructure.ExternalServices.BlobStorage;

/// <summary>
/// Azure Blob Storage implementation for file upload, download, and delete operations.
/// </summary>
internal sealed class BlobService(BlobServiceClient blobServiceClient) : IBlobService
{
    private const string ContainerName = "import-files";
    private const string DefaultContentType = "application/octet-stream";

    /// <summary>
    /// Uploads a stream to blob storage and returns generated file identifier.
    /// </summary>
    public async Task<Guid> UploadAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetContainerClientAsync(cancellationToken);

        var fileId = Guid.NewGuid();
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? DefaultContentType : contentType
                }
            },
            cancellationToken);

        return fileId;
    }

    /// <summary>
    /// Downloads a file stream and content type by identifier.
    /// </summary>
    public async Task<FileResponse> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetContainerClientAsync(cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());

        try
        {
            Response<BlobDownloadStreamingResult> response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            string contentType = string.IsNullOrWhiteSpace(response.Value.Details.ContentType)
                ? DefaultContentType
                : response.Value.Details.ContentType;

            return new FileResponse(response.Value.Content, contentType);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new FileNotFoundException($"File with id '{fileId}' was not found in blob storage.", exception);
        }
    }

    /// <summary>
    /// Deletes a file by identifier if it exists.
    /// </summary>
    public async Task DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetContainerClientAsync(cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        return containerClient;
    }
}
