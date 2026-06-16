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
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ownSupplementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<DeleteOwnSupplementResponse>> DeleteWithCascadeAsync(
        Guid userId,
        Guid ownSupplementId,
        CancellationToken cancellationToken);
}
