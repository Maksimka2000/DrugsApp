using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;

/// <summary>
/// Application service coordinating own supplement cascade deletion.
/// </summary>
public sealed class ClientOwnSupplementDeletionApplicationService(
    IClientOwnSupplementRepository _clientOwnSupplementRepository) : IClientOwnSupplementDeletionApplicationService
{
    /// <summary>
    /// Deletes an own supplement with cascade, returning counts of related deletions.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="ownSupplementId">Own supplement identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Deletion response or error.</returns>
    public async Task<ErrorOr<DeleteOwnSupplementResponse>> DeleteWithCascadeAsync(
        Guid userId,
        Guid ownSupplementId,
        CancellationToken cancellationToken)
    {
        Domain.RegularSupplementsFeature.Entities.ClientOwnSupplement? ownSupplement =
            await _clientOwnSupplementRepository.GetTrackedByIdForUserWithMembersAsync(
                userId,
                ownSupplementId,
                cancellationToken);
        if (ownSupplement is null)
        {
            return Error.NotFound(
                description: "Own supplement not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientOwnSupplement",
                    ["resourceId"] = ownSupplementId.ToString(),
                });
        }

        (int LiveCount, int ExpiredCount) referencingCounts =
            await _clientOwnSupplementRepository.CountReferencingRegularSupplementsAsync(
                ownSupplementId,
                cancellationToken);

        _clientOwnSupplementRepository.Remove(ownSupplement);

        return new DeleteOwnSupplementResponse(
            DeletedRegularSupplementCount: referencingCounts.LiveCount,
            DeletedHistoryCount: referencingCounts.ExpiredCount);
    }
}
