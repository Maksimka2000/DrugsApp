using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.AccountFeature.Interfaces;

/// <summary>
/// Admin-initiated client account removal without password verification.
/// </summary>
public interface IClientAccountAdminDeletionService
{
    /// <summary>
    /// Deletes a client-portal account and all owned client data.
    /// </summary>
    Task<ErrorOr<Success>> DeleteByAdminAsync(
        Guid clientUserId,
        CancellationToken cancellationToken);
}
