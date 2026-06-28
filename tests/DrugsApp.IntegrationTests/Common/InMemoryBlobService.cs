using System.Collections.Concurrent;
using DrugsApp.Domain.Common;

namespace DrugsApp.IntegrationTests.Common;

internal sealed class InMemoryBlobService : IBlobService
{
    private readonly ConcurrentDictionary<Guid, (byte[] Content, string ContentType)> _files = new();

    public async Task<Guid> UploadAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        Guid fileId = Guid.NewGuid();
        _files[fileId] = (memoryStream.ToArray(), contentType);
        return fileId;
    }

    public Task<FileResponse> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        if (!_files.TryGetValue(fileId, out (byte[] Content, string ContentType) entry))
        {
            throw new FileNotFoundException($"File with id '{fileId}' was not found in in-memory blob storage.");
        }

        return Task.FromResult(new FileResponse(new MemoryStream(entry.Content), entry.ContentType));
    }

    public Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        _files.TryRemove(fileId, out _);
        return Task.CompletedTask;
    }
}
