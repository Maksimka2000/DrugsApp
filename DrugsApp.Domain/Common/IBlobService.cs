using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.Common;

/// <summary>
/// Contract for storing, retrieving, and deleting binary files in blob storage.
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// Uploads a file stream and returns generated file identifier.
    /// </summary>
    /// <param name="stream">Source file stream.</param>
    /// <param name="contentType">HTTP content type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated file identifier.</returns>
    Task<Guid> UploadAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file stream and metadata for the provided file identifier.
    /// </summary>
    /// <param name="fileId">File identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File stream response model.</returns>
    Task<FileResponse> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes file if it exists.
    /// </summary>
    /// <param name="fileId">File identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Blob file response model containing stream and content type.
/// </summary>
/// <param name="Stream">Readable file stream.</param>
/// <param name="ContentType">HTTP content type.</param>
public sealed record FileResponse(
    Stream Stream,
    string ContentType);
