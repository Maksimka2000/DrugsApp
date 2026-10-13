using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SystemTextFeature.Entities;

namespace DrugsApp.Domain.SystemTextFeature.Repositories;

/// <summary>
/// Repository contract for system text aggregates.
/// </summary>
public interface ISystemTextRepository
{
    Task<SystemText?> GetByKeyAsync(string key, CancellationToken cancellationToken);

    Task InvalidateAsync(string key, CancellationToken cancellationToken);
}
