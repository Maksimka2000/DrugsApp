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
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.InteractionFeature.SearchInteractions;

/// <summary>
/// Represents a query to search interactions based on the specified criteria.
/// </summary>
/// <param name="InteractionKind">
/// Specifies the type of interaction to search for, such as Substance, Food, or Supplement.
/// </param>
/// <param name="LeftQuery">
/// The search term or identifier used for the left-side entity in the interaction query.
/// </param>
/// <param name="RightQuery">
/// The search term or identifier used for the right-side entity in the interaction query.
/// </param>
/// <param name="SortBy">
/// Specifies the sorting criteria for the interactions, such as Severity, CreatedAt, or UpdatedAt.
/// </param>
/// <param name="Ascending">
/// A boolean indicating whether the sort order should be ascending. False indicates descending order.
/// </param>
/// <param name="Skip">
/// The number of records to skip in the search results for pagination purposes.
/// </param>
/// <param name="Take">
/// The maximum number of records to retrieve in the search results.
/// </param>
public sealed record SearchInteractionsQuery(
    InteractionKind InteractionKind,
    string? LeftQuery,
    string? RightQuery,
    InteractionSortBy SortBy,
    bool Ascending,
    int Skip,
    int Take) : IRequest<ErrorOr<SearchInteractionsResponse>>;

/// <summary>
/// Represents the response received when searching for interactions based on the provided query.
/// </summary>
/// <param name="Items">
/// A collection of interaction cards containing detailed information about the interactions
/// matching the search criteria.
/// </param>
/// <param name="TotalCount">
/// The total number of interactions found that match the search criteria. This value may be null
/// if the total count is not provided or applicable.
/// </param>
/// <param name="HasMore">
/// A boolean indicating whether there are more interactions available beyond the current result set.
/// </param>
/// <param name="LeftQueryFound">
/// Information about the left-side query of the interaction, including its type, ID, label, and
/// members if any were matched in the search.
/// </param>
/// <param name="RightQueryFound">
/// Information about the right-side query of the interaction, including its type, ID, label, and
/// members if any were matched in the search.
/// </param>
public sealed record SearchInteractionsResponse(
    IReadOnlyCollection<InteractionCardDto> Items,
    int? TotalCount,
    bool HasMore,
    InteractionQueryFoundDto? LeftQueryFound,
    InteractionQueryFoundDto? RightQueryFound);

internal sealed class SearchInteractionsHandler(
    IInteractionRepository _interactionRepository,
    IActiveSubstanceRepository _activeSubstanceRepository,
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IFoodRepository _foodRepository,
    IFoodGroupRepository _foodGroupRepository,
    ISupplementRepository _supplementRepository,
    ISupplementGroupRepository _supplementGroupRepository)
    : IRequestHandler<SearchInteractionsQuery, ErrorOr<SearchInteractionsResponse>>
{
    public async Task<ErrorOr<SearchInteractionsResponse>> Handle(
        SearchInteractionsQuery query,
        CancellationToken cancellationToken)
    {
        // resolve left party. This is the substance side. If 
        PartyResolutionResult leftResolution = await ResolveSubstanceSideAsync(query.LeftQuery, cancellationToken);
        // if the left party is not found, return an empty response.
        if (leftResolution.ShouldReturnEmpty)
        {
            return BuildEmptyResponse(leftResolution, null);
        }

        // if the interaction kind is food or supplement, resolve the right party. This is the food or supplement side.
        PartyResolutionResult rightResolution = query.InteractionKind switch
        {
            InteractionKind.Food => await ResolveFoodSideAsync(query.RightQuery, cancellationToken),
            InteractionKind.Supplement => await ResolveSupplementSideAsync(query.RightQuery, cancellationToken),
            _ => await ResolveSubstanceSideAsync(query.RightQuery, cancellationToken)
        };
        // if the right party is not found, return an empty response.
        if (rightResolution.ShouldReturnEmpty)
        {
            return BuildEmptyResponse(leftResolution, rightResolution);
        }

        // initialize take plus one (use all the time )
        int takePlusOne = query.Take + 1;
        // initialize total count (use all the time )
        int? totalCount = null;


        // initialize search result list (use all the time )
        List<Interaction> rows = new();

        // search 
        rows = await _interactionRepository.SearchByPartiesAsync(
            query.InteractionKind,
            leftResolution.PartyKey,
            rightResolution.PartyKey,
            query.SortBy,
            query.Ascending,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = rows.Count > query.Take;
        // get page
        var page = rows.Take(query.Take).ToList();

        IReadOnlyDictionary<InteractionPartyKey, InteractionQueryFoundDto> partyLookup =
            await SearchInteractionsPartyMapper.BuildPartyLookupAsync(
                page,
                _activeSubstanceRepository,
                _activeSubstanceGroupRepository,
                _foodRepository,
                _foodGroupRepository,
                _supplementRepository,
                _supplementGroupRepository,
                cancellationToken);

        var items = page
            .Select(interaction => SearchInteractionsPartyMapper.MapToCard(
                interaction,
                query.InteractionKind,
                partyLookup))
            .ToList();

        // count total items
        if (query.Skip == 0)
        {
            // if the skip is 0, count total items
            totalCount = rows.Count == 0 ? 0 : await _interactionRepository.CountByPartiesAsync(
                query.InteractionKind,
                leftResolution.PartyKey,
                rightResolution.PartyKey,
                cancellationToken);
        }

        return new SearchInteractionsResponse(
            Items: items,
            TotalCount: totalCount,
            HasMore: hasMore,
            LeftQueryFound: leftResolution.ResolvedDto,
            RightQueryFound: rightResolution.ResolvedDto);
    }

    private static SearchInteractionsResponse BuildEmptyResponse(
        PartyResolutionResult left,
        PartyResolutionResult? right)
    {
        return new SearchInteractionsResponse(
            Items: Array.Empty<InteractionCardDto>(),
            TotalCount: 0,
            HasMore: false,
            LeftQueryFound: left.ResolvedDto,
            RightQueryFound: right?.ResolvedDto);
    }

    private async Task<PartyResolutionResult> ResolveSubstanceSideAsync(
        string? query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return PartyResolutionResult.Unspecified;
        }

        ActiveSubstance? substance = await _activeSubstanceRepository.GetByNameAsync(query, cancellationToken);
        if (substance is not null)
        {
            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.Substance, substance.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.Substance,
                    substance.Id,
                    substance.ActiveSubstanceName,
                    null),
                AtomicType: ShortInteractionType.Substance,
                AtomicIds: new[] { substance.Id });
        }

        ActiveSubstanceGroup? group = await _activeSubstanceGroupRepository.GetByNameAsync(query, cancellationToken);
        if (group is not null)
        {
            var members = group.ActiveSubstances
                .Select(member => new IdLabelDto(member.Id, member.ActiveSubstanceName))
                .ToList();

            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.SubstanceGroup, group.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.SubstanceGroup,
                    group.Id,
                    group.Name,
                    members),
                AtomicType: ShortInteractionType.Substance,
                AtomicIds: members.Select(member => member.Id).ToList());
        }

        return PartyResolutionResult.NotFound;
    }

    private async Task<PartyResolutionResult> ResolveFoodSideAsync(
        string? query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return PartyResolutionResult.Unspecified;
        }

        Food? food = await _foodRepository.GetByNameAsync(query, cancellationToken);
        if (food is not null)
        {
            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.Food, food.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.Food,
                    food.Id,
                    food.Name,
                    null),
                AtomicType: ShortInteractionType.Food,
                AtomicIds: new[] { food.Id });
        }

        FoodGroup? foodGroup = await _foodGroupRepository.GetByNameAsync(query, cancellationToken);
        if (foodGroup is not null)
        {
            var members = foodGroup.Foods
                .Select(member => new IdLabelDto(member.Id, member.Name))
                .ToList();

            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.FoodGroup, foodGroup.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.FoodGroup,
                    foodGroup.Id,
                    foodGroup.Name,
                    members),
                AtomicType: ShortInteractionType.Food,
                AtomicIds: members.Select(member => member.Id).ToList());
        }

        return PartyResolutionResult.NotFound;
    }

    private async Task<PartyResolutionResult> ResolveSupplementSideAsync(
        string? query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return PartyResolutionResult.Unspecified;
        }

        Supplement? supplement = await _supplementRepository.GetByNameAsync(query, cancellationToken);
        if (supplement is not null)
        {
            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.Supplement, supplement.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.Supplement,
                    supplement.Id,
                    supplement.Name,
                    null),
                AtomicType: ShortInteractionType.Supplement,
                AtomicIds: new[] { supplement.Id });
        }

        SupplementGroup? supplementGroup = await _supplementGroupRepository.GetByNameAsync(query, cancellationToken);
        if (supplementGroup is not null)
        {
            var members = supplementGroup.Supplements
                .Select(member => new IdLabelDto(member.Id, member.Name))
                .ToList();

            return new PartyResolutionResult(
                HasQuery: true,
                Found: true,
                PartyKey: new InteractionPartyKey(InteractionType.SupplementGroup, supplementGroup.Id),
                ResolvedDto: new InteractionQueryFoundDto(
                    InteractionType.SupplementGroup,
                    supplementGroup.Id,
                    supplementGroup.Name ?? string.Empty,
                    members),
                AtomicType: ShortInteractionType.Supplement,
                AtomicIds: members.Select(member => member.Id).ToList());
        }

        return PartyResolutionResult.NotFound;
    }
    

    private sealed record PartyResolutionResult(
        bool HasQuery,
        bool Found,
        InteractionPartyKey? PartyKey,
        InteractionQueryFoundDto? ResolvedDto,
        ShortInteractionType? AtomicType,
        IReadOnlyCollection<Guid> AtomicIds)
    {
        public static PartyResolutionResult Unspecified =>
            new(false, true, null, null, null, Array.Empty<Guid>());

        public static PartyResolutionResult NotFound =>
            new(true, false, null, null, null, Array.Empty<Guid>());

        public bool ShouldReturnEmpty => HasQuery && !Found;
    }
}
