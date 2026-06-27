using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;

/// <summary>
/// Orchestrates own supplement deletion with cascade removal of referencing schedules.
/// </summary>
public interface IClientOwnSupplementDeletionApplicationService
{
    /// <summary>
    /// Deletes an own supplement and cascades removal to referencing schedules.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="ownSupplementId">Own supplement identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Deletion response or errors.</returns>
    Task<ErrorOr<DeleteOwnSupplementResponse>> DeleteWithCascadeAsync(
        Guid userId,
        Guid ownSupplementId,
        CancellationToken cancellationToken);
}
