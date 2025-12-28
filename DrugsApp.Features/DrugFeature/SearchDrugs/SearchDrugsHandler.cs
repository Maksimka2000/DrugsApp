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
internal sealed class SearchDrugsHandler : IRequestHandler<SearchDrugsQuery, ErrorOr<SearchDrugsResponse>>
{
    private static readonly IReadOnlyList<(DrugSearchField Field, string Token)> SearchPriority = new List<(DrugSearchField, string)>
    {
        (DrugSearchField.CommercialName, "commercial_name"),
        (DrugSearchField.ActiveSubstance, "active_substance"),
        (DrugSearchField.AtcCode, "atc_code"),
        (DrugSearchField.RouteOfAdministration, "route_of_administration"),
        (DrugSearchField.PharmaceuticalForm, "pharmaceutical_form"),
        (DrugSearchField.ProductIdentifier, "product_identifier")
    };

    private static readonly IReadOnlyList<string> SearchPriorityTokens = new ReadOnlyCollection<string>(
        SearchPriority.Select(static entry => entry.Token).ToList());

    private static readonly Dictionary<string, DrugSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["commercial_name"] = DrugSortField.CommercialName,
        ["created_at"] = DrugSortField.CreatedAt,
        ["updated_at"] = DrugSortField.UpdatedAt
    };

    private readonly IDrugRepository _drugRepository;

    public SearchDrugsHandler(IDrugRepository drugRepository)
    {
        _drugRepository = drugRepository;
    }

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
        // determine sort order
        bool ascending = query.OrderBy.Equals("ascending", System.StringComparison.OrdinalIgnoreCase);
        // get sort field
        DrugSortField sortField = SortFieldMap.GetValueOrDefault(query.SortBy, DrugSortField.CommercialName);
        // build type filter
        IReadOnlyCollection<DrugType>? selectedTypesFilter = BuildTypeFilter(query.TypeOfDrugs);
        // initialize search result list
        List<Drug> items;
        // initialize matched field token
        string? matchedFieldToken = null;
        // initialize counter
        int? totalCount = null;
        // initialize applied search field
        DrugSearchField appliedSearchField = DrugSearchField.None;

        if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
        {
            items = await _drugRepository.SearchAsync(
                searchQuery: null,
                typeOfDrugsFilter: selectedTypesFilter,
                searchField: DrugSearchField.None,
                sortField: sortField,
                ascending: ascending,
                skip: query.Skip,
                take: query.Take + 1,
                cancellationToken: cancellationToken);
        }
        else
        {
            items = new List<Drug>();
            foreach ((DrugSearchField field, string token) in SearchPriority)
            {
                List<Drug> attempt = await _drugRepository.SearchAsync(
                    searchQuery: normalizedSearchQuery,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: field,
                    sortField: sortField,
                    ascending: ascending,
                    skip: query.Skip,
                    take: query.Take + 1,
                    cancellationToken: cancellationToken);

                if (attempt.Count > 0)
                {
                    items = attempt;
                    matchedFieldToken = token;
                    appliedSearchField = field;
                    break;
                }
            }
        }

        bool hasMore = items.Count > query.Take;
        if (hasMore)
        {
            items = items.GetRange(0, query.Take);
        }

        if (query.Skip == 0)
        {
            if (items.Count == 0){
                totalCount = 0;
            } else if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
            {
                totalCount = await _drugRepository.CountAsync(
                    searchQuery: null,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: DrugSearchField.None,
                    cancellationToken: cancellationToken);
            } else
            {
                totalCount = await _drugRepository.CountAsync(
                    searchQuery: normalizedSearchQuery,
                    typeOfDrugsFilter: selectedTypesFilter,
                    searchField: appliedSearchField,
                    cancellationToken: cancellationToken);
            }
        }

        SearchDrugsMetadata metadata = BuildMetadata(
            normalizedSearchQuery,
            matchedFieldToken,
            items.Count > 0,
            SearchPriorityTokens);

        List<DrugDto> mapped = items.MapToDtoList();
        return SearchDrugsMapping.MapToResponse(mapped, totalCount, hasMore, metadata);
    }

    private static IReadOnlyCollection<DrugType>? BuildTypeFilter(List<string>? requestedTypes)
    {
        if (requestedTypes != null)
        {
            IReadOnlyCollection<DrugType> parsed = requestedTypes
                .Select(type => Enum.Parse<DrugType>(type, ignoreCase: true))
                .Distinct()
                .ToList()
                .AsReadOnly();

            return parsed;
        }
        return null;
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
