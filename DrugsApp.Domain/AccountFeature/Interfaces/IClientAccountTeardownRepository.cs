using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AccountFeature.Interfaces;

/// <summary>
/// Bulk removal of client-owned medical and social data before Identity user deletion.
/// </summary>
public interface IClientAccountTeardownRepository
{
    /// <summary>
    /// Deletes all client-owned domain rows linked to the user identifier.
    /// </summary>
    Task DeleteAllOwnedClientDataAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
