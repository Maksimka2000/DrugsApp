using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.SearchDrugs;

/// <summary>
/// Handles searching and listing drugs with paging metadata.
/// </summary>
internal sealed class SearchDrugsHandler(IDrugRepository _drugRepository) : IRequestHandler<SearchDrugsQuery, ErrorOr<SearchDrugsResponse>>
{
    private static readonly Dictionary<string, DrugSearchField> SearchPriority = new()
        {
            ["commercial_name"] = DrugSearchField.CommercialName,
            ["active_substance"] = DrugSearchField.ActiveSubstance,
            ["atc_code" ] = DrugSearchField.AtcCode,
            ["route_of_administration"] = DrugSearchField.RouteOfAdministration,
            ["pharmaceutical_form"] = DrugSearchField.PharmaceuticalForm,
            ["product_identifier"] = DrugSearchField.ProductIdentifier
        };

    private static readonly Dictionary<string, DrugSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["commercial_name"] = DrugSortField.CommercialName,
        ["created_at"] = DrugSortField.CreatedAt,
        ["updated_at"] = DrugSortField.UpdatedAt
    };

    private static readonly Dictionary<string, DrugType> TypeOfDrugMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["standard"] = DrugType.Standard,
        ["ophthalmic"] = DrugType.Ophthalmic,
        ["special"] = DrugType.Special
    };


    /// <summary>
    /// Searches drugs with stable sorting and returns paging envelope.
    /// </summary>
    /// <param name="query">Search query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items, total count (first page only), and has_more flag.</returns>
    public async Task<ErrorOr<SearchDrugsResponse>> Handle(
        SearchDrugsQuery query,
        CancellationToken cancellationToken)
    {
        // normalize search query
        string? normalizedSearchQuery = NormalizeSearchQuery(query.SearchQuery);
        // normalize type of drugs filter
        IReadOnlyCollection<DrugType>? selectedTypesFilter = NormalizeTypeOfDrugFilter(query.TypeOfDrugs);
        // determine sort order
        bool ascending = query.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase);
        // get sort field
        DrugSortField sortField = SortFieldMap.GetValueOrDefault(query.SortBy.Trim(), DrugSortField.CommercialName);
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize counter
        int? totalCount = null;

        // initialize applied search field
        DrugSearchField appliedSearchField = DrugSearchField.None;
        // initialize search result list
        List<Drug> drugs;
        // initialize matched field token
        string? matchedFieldToken = null;
        // initialize  matchedFiledtoken list
        List<string> matchedFieldTokens = new();

        // search drugs if search query is empty
        if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
        {
            drugs = await _drugRepository.SearchAsync(
                searchQuery: null,
                typeOfDrugsFilter: selectedTypesFilter,
                searchField: DrugSearchField.None,
                sortField: sortField,
                ascending: ascending,
                skip: query.Skip,
                take: takePlusOne,
                cancellationToken: cancellationToken);
        }
        else
        {
            // search drugs if search query is not empty
            drugs = new List<Drug>();
            foreach ((string token, DrugSearchField field) in SearchPriority)
            {
                List<Drug> attempt = await _drugRepository.SearchAsync(
                    searchQuery: normalizedSearchQuery,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: field,
                    sortField: sortField,
                    ascending: ascending,
                    skip: query.Skip,
                    take: takePlusOne,
                    cancellationToken: cancellationToken);

                if (attempt.Count > 0)
                {
                    drugs = attempt;
                    matchedFieldToken = token;
                    matchedFieldTokens.Add(token);
                    appliedSearchField = field;
                    break;
                }
            }
        }

        // determine if there are more items
        bool hasMore = drugs.Count > query.Take;

        // get page
        List<Drug> page = hasMore
            ? drugs.Take(query.Take).ToList()
            : drugs;

        // map to DTOs
        List<DrugDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            if (drugs.Count == 0)
            {
                totalCount = 0;
            }
            else if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
            {
                totalCount = await _drugRepository.CountAsync(
                    searchQuery: null,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: DrugSearchField.None,
                    cancellationToken: cancellationToken);
            }
            else
            {
                totalCount = await _drugRepository.CountAsync(
                    searchQuery: normalizedSearchQuery,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: appliedSearchField,
                    cancellationToken: cancellationToken);
            }
        }

        // build metadata
        SearchDrugsMetadata metadata = BuildMetadata(
            normalizedSearchQuery,
            matchedFieldToken,
            items.Count > 0,
            matchedFieldTokens.AsReadOnly());


        return items.MapToResponse(totalCount, hasMore, metadata);
    }

    private static IReadOnlyCollection<DrugType>? NormalizeTypeOfDrugFilter(List<string>? requestedTypes)
    {
        if (requestedTypes is null || requestedTypes.Count == 0)
        {
            return null;
        }

        IReadOnlyCollection<DrugType> distinct = requestedTypes
            .Select(status => TypeOfDrugMap.TryGetValue(status, out DrugType mapped) ? mapped : (DrugType?)null)
            .Where(mapped => mapped.HasValue)
            .Select(mapped => mapped!.Value)
            .Distinct()
            .ToList()
            .AsReadOnly();

        if (distinct.Count == 0)
        {
            return null;
        }

        var all = Enum.GetValues<DrugType>().ToHashSet();

        bool selectsAll = all.SetEquals(distinct); 
        
        return selectsAll ? null : distinct.ToList().AsReadOnly();
    }

    private static string? NormalizeSearchQuery(string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return null;
        }

        string[] parts = searchQuery
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 0 ? null : string.Join(' ', parts);
    }

    private static SearchDrugsMetadata BuildMetadata(
        string? normalizedSearchQuery,
        string? matchedFieldToken,
        bool foundAny,
        IReadOnlyList<string> attemptedFields)
    {
        if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
        {
            return new SearchDrugsMetadata(
                MatchedField: null,
                UsedFallback: false,
                AttemptedFields: null,
                Message: "Search executed without query; returning default listing ordered by commercial_name.");
        }

        if (!foundAny)
        {
            string attempted = string.Join(", ", attemptedFields);
            return new SearchDrugsMetadata(
                MatchedField: null,
                UsedFallback: true,
                AttemptedFields: attemptedFields,
                Message: $"No results matched the search query across {attempted}.");
        }

        bool usedFallback = !string.Equals(matchedFieldToken, "commercial_name", StringComparison.OrdinalIgnoreCase);
        matchedFieldToken ??= "commercial_name";
        string message = usedFallback
            ? $"Search query matched no commercial_name entries; results were retrieved by {matchedFieldToken}."
            : "Results matched by commercial_name.";

        return new SearchDrugsMetadata(
            MatchedField: matchedFieldToken,
            UsedFallback: usedFallback,
            AttemptedFields: attemptedFields,
            Message: message);
    }
}
