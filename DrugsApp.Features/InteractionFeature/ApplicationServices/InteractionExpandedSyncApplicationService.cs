using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.InteractionFeature.ApplicationServices;

/// <summary>
/// Request payload for synchronizing expanded interaction rows after a group membership change.
/// </summary>
public sealed record SyncInteractionExpandedAfterGroupMembershipChangedRequest(
    InteractionType GroupPartyType,
    Guid GroupId,
    IReadOnlyCollection<Guid> AddedMemberIds,
    IReadOnlyCollection<Guid> RemovedMemberIds,
    IReadOnlyCollection<Guid> CurrentMemberIds);


/// <summary>
/// Application service for synchronizing interaction expanded rows after group membership changes.
/// </summary>
internal sealed class InteractionExpandedSyncApplicationService(
    IInteractionRepository _interactionRepository,
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IFoodGroupRepository _foodGroupRepository,
    ISupplementGroupRepository _supplementGroupRepository)
    : IInteractionExpandedSyncApplicationService
{
    public async Task<ErrorOr<Success>> SyncAfterGroupMembershipChangedAsync(
        SyncInteractionExpandedAfterGroupMembershipChangedRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AddedMemberIds.Count == 0 && request.RemovedMemberIds.Count == 0)
        {
            return Result.Success;
        }

        if (!IsGroupType(request.GroupPartyType))
        {
            return Error.Validation(
                description: "GroupPartyType must be a group interaction type.",
                metadata: new Dictionary<string, object>
                {
                    ["rule"] = "GroupPartyType"
                });
        }

        bool participates = await _interactionRepository.AnyPartyReferencesGroupAsync(
            request.GroupPartyType,
            request.GroupId,
            cancellationToken);

        if (!participates)
        {
            return Result.Success;
        }

        IReadOnlyCollection<Guid> interactionIds = await _interactionRepository.GetInteractionIdsByGroupPartyAsync(
            request.GroupPartyType,
            request.GroupId,
            cancellationToken);

        if (interactionIds.Count == 0)
        {
            return Result.Success;
        }

        Dictionary<Guid, IReadOnlyCollection<InteractionParty>> partiesByInteraction =
            await _interactionRepository.GetPartiesByInteractionIdsAsync(interactionIds, cancellationToken);

        Dictionary<(InteractionType PartyType, Guid Id), IReadOnlyCollection<Guid>> groupMembersCache = new();

        foreach ((Guid interactionId, IReadOnlyCollection<InteractionParty> parties) in partiesByInteraction)
        {
            InteractionParty? groupParty = parties.FirstOrDefault(party =>
                party.PartyType == request.GroupPartyType && GetPartyId(party) == request.GroupId);
            InteractionParty? otherParty = parties.FirstOrDefault(party => party != groupParty);

            if (groupParty is null || otherParty is null)
            {
                continue;
            }

            ShortInteractionType groupAtomicType = MapToShortType(request.GroupPartyType);
            IReadOnlyCollection<Guid> otherAtomicIds = await ResolveAtomicIdsAsync(
                otherParty,
                groupMembersCache,
                cancellationToken);
            ShortInteractionType otherAtomicType = MapToShortType(otherParty.PartyType);

            if (otherAtomicIds.Count == 0)
            {
                continue;
            }

            List<InteractionAtomicPair> removedPairs = InteractionAtomicPairs.BuildPairs(
                groupAtomicType,
                request.RemovedMemberIds,
                otherAtomicType,
                otherAtomicIds);

            if (removedPairs.Count > 0 && groupAtomicType == otherAtomicType)
            {
                HashSet<InteractionAtomicPair> stillJustifiedPairs = InteractionAtomicPairs
                    .BuildPairs(groupAtomicType, request.CurrentMemberIds, otherAtomicType, otherAtomicIds)
                    .ToHashSet();

                removedPairs.RemoveAll(stillJustifiedPairs.Contains);
            }

            if (removedPairs.Count > 0)
            {
                await _interactionRepository.DeleteExpandedRowsByAtomicPairsAsync(
                    interactionId,
                    removedPairs,
                    cancellationToken);
            }

            List<InteractionAtomicPair> addedPairs = InteractionAtomicPairs.BuildPairs(
                groupAtomicType,
                request.AddedMemberIds,
                otherAtomicType,
                otherAtomicIds);

            if (addedPairs.Count > 0)
            {
                await _interactionRepository.InsertExpandedRowsIgnoreConflictsAsync(
                    interactionId,
                    addedPairs,
                    cancellationToken);
            }
        }

        return Result.Success;
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveAtomicIdsAsync(
        InteractionParty party,
        Dictionary<(InteractionType PartyType, Guid Id), IReadOnlyCollection<Guid>> cache,
        CancellationToken cancellationToken)
    {
        (InteractionType PartyType, Guid Id) key = (party.PartyType, GetPartyId(party) ?? Guid.Empty);
        if (key.Id == Guid.Empty)
        {
            return Array.Empty<Guid>();
        }

        if (cache.TryGetValue(key, out IReadOnlyCollection<Guid>? cached))
        {
            return cached;
        }

        IReadOnlyCollection<Guid> resolved = party.PartyType switch
        {
            InteractionType.Substance => new[] { party.ActiveSubstanceId!.Value },
            InteractionType.Food => new[] { party.FoodId!.Value },
            InteractionType.Supplement => new[] { party.SupplementId!.Value },
            InteractionType.SubstanceGroup => await ResolveActiveSubstanceGroupMembersAsync(party.ActiveSubstanceGroupId!.Value, cancellationToken),
            InteractionType.FoodGroup => await ResolveFoodGroupMembersAsync(party.FoodGroupId!.Value, cancellationToken),
            InteractionType.SupplementGroup => await ResolveSupplementGroupMembersAsync(party.SupplementGroupId!.Value, cancellationToken),
            _ => Array.Empty<Guid>()
        };

        cache[key] = resolved;
        return resolved;
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveActiveSubstanceGroupMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        ActiveSubstanceGroup? group = await _activeSubstanceGroupRepository.GetByIdAsync(groupId, cancellationToken);
        return group?.ActiveSubstances.Select(member => member.Id).ToList() ?? new List<Guid>();
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveFoodGroupMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        FoodGroup? group = await _foodGroupRepository.GetByIdAsync(groupId, cancellationToken);
        return group?.Foods.Select(member => member.Id).ToList() ?? new List<Guid>();
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveSupplementGroupMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(groupId, cancellationToken);
        return group?.Supplements.Select(member => member.Id).ToList() ?? new List<Guid>();
    }

    private static bool IsGroupType(InteractionType partyType)
    {
        return partyType is InteractionType.SubstanceGroup
            or InteractionType.FoodGroup
            or InteractionType.SupplementGroup;
    }

    private static ShortInteractionType MapToShortType(InteractionType partyType)
    {
        return partyType switch
        {
            InteractionType.Substance or InteractionType.SubstanceGroup => ShortInteractionType.Substance,
            InteractionType.Food or InteractionType.FoodGroup => ShortInteractionType.Food,
            InteractionType.Supplement or InteractionType.SupplementGroup => ShortInteractionType.Supplement,
            _ => ShortInteractionType.Substance
        };
    }

    private static Guid? GetPartyId(InteractionParty party)
    {
        return party.PartyType switch
        {
            InteractionType.Substance => party.ActiveSubstanceId,
            InteractionType.SubstanceGroup => party.ActiveSubstanceGroupId,
            InteractionType.Food => party.FoodId,
            InteractionType.FoodGroup => party.FoodGroupId,
            InteractionType.Supplement => party.SupplementId,
            InteractionType.SupplementGroup => party.SupplementGroupId,
            _ => null
        };
    }
}
