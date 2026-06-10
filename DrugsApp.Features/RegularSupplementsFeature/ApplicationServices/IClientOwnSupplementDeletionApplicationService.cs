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
    Task<ErrorOr<DeleteOwnSupplementResponse>> DeleteWithCascadeAsync(
        Guid userId,
        Guid ownSupplementId,
        CancellationToken cancellationToken);
}
