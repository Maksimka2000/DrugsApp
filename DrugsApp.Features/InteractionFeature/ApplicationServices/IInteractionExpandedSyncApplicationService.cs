using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Features.InteractionFeature.ApplicationServices;

/// <summary>
/// Synchronizes expanded rows when group memberships change.
/// </summary>
public interface IInteractionExpandedSyncApplicationService
{
    /// <summary>
    /// Syncs interaction data after group membership changes, such as adding or removing members from a group.
    /// </summary>
    /// <param name="request">
    /// An object containing details about the group membership update, including the type of group,
    /// group identifier, added member identifiers, and removed member identifiers.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation if necessary.
    /// </param>
    /// <returns>
    /// A result indicating success or an error in performing the synchronization.
    /// </returns>
    Task<ErrorOr<Success>> SyncAfterGroupMembershipChangedAsync(
        SyncInteractionExpandedAfterGroupMembershipChangedRequest request,
        CancellationToken cancellationToken);
}


