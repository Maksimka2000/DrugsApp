using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.InteractionFeature.Shared;

namespace DrugsApp.Features.InteractionFeature.SearchInteractions;

internal static class SearchInteractionsPartyMapper
{
    public static async Task<IReadOnlyDictionary<InteractionPartyKey, InteractionQueryFoundDto>> BuildPartyLookupAsync(
        IReadOnlyCollection<Interaction> interactions,
        IActiveSubstanceRepository activeSubstanceRepository,
        IActiveSubstanceGroupRepository activeSubstanceGroupRepository,
        IFoodRepository foodRepository,
        IFoodGroupRepository foodGroupRepository,
        ISupplementRepository supplementRepository,
        ISupplementGroupRepository supplementGroupRepository,
        CancellationToken cancellationToken)
    {
        var substanceIds = new HashSet<Guid>();
        var substanceGroupIds = new HashSet<Guid>();
        var foodIds = new HashSet<Guid>();
        var foodGroupIds = new HashSet<Guid>();
        var supplementIds = new HashSet<Guid>();
        var supplementGroupIds = new HashSet<Guid>();

        foreach (Interaction interaction in interactions)
        {
            foreach (InteractionParty party in interaction.Parties)
            {
                CollectPartyIds(
                    party,
                    substanceIds,
                    substanceGroupIds,
                    foodIds,
                    foodGroupIds,
                    supplementIds,
                    supplementGroupIds);
            }
        }

        var lookup = new Dictionary<InteractionPartyKey, InteractionQueryFoundDto>();

        if (substanceIds.Count > 0)
        {
            IReadOnlyCollection<ActiveSubstance> substances = await activeSubstanceRepository.GetByIdsAsync(
                substanceIds,
                cancellationToken);

            foreach (ActiveSubstance substance in substances)
            {
                lookup[new InteractionPartyKey(InteractionType.Substance, substance.Id)] =
                    new InteractionQueryFoundDto(
                        InteractionType.Substance,
                        substance.Id,
                        substance.ActiveSubstanceName,
                        null);
            }
        }

        if (substanceGroupIds.Count > 0)
        {
            foreach (Guid groupId in substanceGroupIds)
            {
                ActiveSubstanceGroup? group = await activeSubstanceGroupRepository.GetByIdAsync(
                    groupId,
                    cancellationToken);

                if (group is null)
                {
                    continue;
                }

                var members = group.ActiveSubstances
                    .Select(member => new IdLabelDto(member.Id, member.ActiveSubstanceName))
                    .ToList();

                lookup[new InteractionPartyKey(InteractionType.SubstanceGroup, group.Id)] =
                    new InteractionQueryFoundDto(
                        InteractionType.SubstanceGroup,
                        group.Id,
                        group.Name,
                        members);
            }
        }

        if (foodIds.Count > 0)
        {
            List<Food> foods = await foodRepository.GetByIdsAsync(foodIds, cancellationToken);

            foreach (Food food in foods)
            {
                lookup[new InteractionPartyKey(InteractionType.Food, food.Id)] =
                    new InteractionQueryFoundDto(InteractionType.Food, food.Id, food.Name, null);
            }
        }

        if (foodGroupIds.Count > 0)
        {
            List<FoodGroup> foodGroups = await foodGroupRepository.GetByIdsAsync(foodGroupIds, cancellationToken);

            foreach (FoodGroup group in foodGroups)
            {
                var members = group.Foods
                    .Select(member => new IdLabelDto(member.Id, member.Name))
                    .ToList();

                lookup[new InteractionPartyKey(InteractionType.FoodGroup, group.Id)] =
                    new InteractionQueryFoundDto(InteractionType.FoodGroup, group.Id, group.Name, members);
            }
        }

        if (supplementIds.Count > 0)
        {
            List<Supplement> supplements = await supplementRepository.GetByIdsAsync(supplementIds, cancellationToken);

            foreach (Supplement supplement in supplements)
            {
                lookup[new InteractionPartyKey(InteractionType.Supplement, supplement.Id)] =
                    new InteractionQueryFoundDto(InteractionType.Supplement, supplement.Id, supplement.Name, null);
            }
        }

        if (supplementGroupIds.Count > 0)
        {
            List<SupplementGroup> supplementGroups = await supplementGroupRepository.GetByIdsAsync(
                supplementGroupIds,
                cancellationToken);

            foreach (SupplementGroup group in supplementGroups)
            {
                var members = group.Supplements
                    .Select(member => new IdLabelDto(member.Id, member.Name))
                    .ToList();

                lookup[new InteractionPartyKey(InteractionType.SupplementGroup, group.Id)] =
                    new InteractionQueryFoundDto(
                        InteractionType.SupplementGroup,
                        group.Id,
                        group.Name ?? string.Empty,
                        members);
            }
        }

        return lookup;
    }

    public static InteractionCardDto MapToCard(
        Interaction interaction,
        InteractionKind interactionKind,
        IReadOnlyDictionary<InteractionPartyKey, InteractionQueryFoundDto> partyLookup)
    {
        (InteractionParty leftParty, InteractionParty rightParty) = OrderParties(interaction.Parties, interactionKind);

        InteractionQueryFoundDto left = ResolvePartyDto(leftParty, partyLookup);
        InteractionQueryFoundDto right = ResolvePartyDto(rightParty, partyLookup);

        return interaction.MapToCardDto(left, right, interaction.InteractionSeverity.MapToDto());
    }

    private static InteractionQueryFoundDto ResolvePartyDto(
        InteractionParty party,
        IReadOnlyDictionary<InteractionPartyKey, InteractionQueryFoundDto> partyLookup)
    {
        InteractionPartyKey key = GetPartyKey(party);

        if (partyLookup.TryGetValue(key, out InteractionQueryFoundDto? dto))
        {
            return dto;
        }

        return new InteractionQueryFoundDto(key.PartyType, key.PartyId, string.Empty, null);
    }

    private static (InteractionParty Left, InteractionParty Right) OrderParties(
        IReadOnlyCollection<InteractionParty> parties,
        InteractionKind interactionKind)
    {
        var partyList = parties.ToList();

        if (partyList.Count != 2)
        {
            throw new InvalidOperationException(
                $"Interaction must have exactly 2 parties, found {partyList.Count}.");
        }

        InteractionParty first = partyList[0];
        InteractionParty second = partyList[1];

        if (interactionKind is InteractionKind.Food or InteractionKind.Supplement)
        {
            bool firstIsSubstance = IsSubstanceFamily(first.PartyType);
            bool secondIsSubstance = IsSubstanceFamily(second.PartyType);

            if (firstIsSubstance && !secondIsSubstance)
            {
                return (first, second);
            }

            if (!firstIsSubstance && secondIsSubstance)
            {
                return (second, first);
            }
        }

        InteractionPartyKey firstKey = GetPartyKey(first);
        InteractionPartyKey secondKey = GetPartyKey(second);

        bool keepOrder = firstKey.PartyType < secondKey.PartyType ||
            firstKey.PartyType == secondKey.PartyType &&
            firstKey.PartyId.CompareTo(secondKey.PartyId) <= 0;

        return keepOrder ? (first, second) : (second, first);
    }

    private static bool IsSubstanceFamily(InteractionType partyType) =>
        partyType is InteractionType.Substance or InteractionType.SubstanceGroup;

    private static InteractionPartyKey GetPartyKey(InteractionParty party)
    {
        Guid partyId = party.PartyType switch
        {
            InteractionType.Substance => party.ActiveSubstanceId!.Value,
            InteractionType.SubstanceGroup => party.ActiveSubstanceGroupId!.Value,
            InteractionType.Food => party.FoodId!.Value,
            InteractionType.FoodGroup => party.FoodGroupId!.Value,
            InteractionType.Supplement => party.SupplementId!.Value,
            InteractionType.SupplementGroup => party.SupplementGroupId!.Value,
            _ => throw new InvalidOperationException($"Unsupported interaction party type: {party.PartyType}.")
        };

        return new InteractionPartyKey(party.PartyType, partyId);
    }

    private static void CollectPartyIds(
        InteractionParty party,
        ISet<Guid> substanceIds,
        ISet<Guid> substanceGroupIds,
        ISet<Guid> foodIds,
        ISet<Guid> foodGroupIds,
        ISet<Guid> supplementIds,
        ISet<Guid> supplementGroupIds)
    {
        switch (party.PartyType)
        {
            case InteractionType.Substance when party.ActiveSubstanceId is Guid substanceId:
                substanceIds.Add(substanceId);
                break;
            case InteractionType.SubstanceGroup when party.ActiveSubstanceGroupId is Guid substanceGroupId:
                substanceGroupIds.Add(substanceGroupId);
                break;
            case InteractionType.Food when party.FoodId is Guid foodId:
                foodIds.Add(foodId);
                break;
            case InteractionType.FoodGroup when party.FoodGroupId is Guid foodGroupId:
                foodGroupIds.Add(foodGroupId);
                break;
            case InteractionType.Supplement when party.SupplementId is Guid supplementId:
                supplementIds.Add(supplementId);
                break;
            case InteractionType.SupplementGroup when party.SupplementGroupId is Guid supplementGroupId:
                supplementGroupIds.Add(supplementGroupId);
                break;
        }
    }
}
