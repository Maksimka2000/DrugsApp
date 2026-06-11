using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.SearchDrugForms;

/// <summary>
/// Handles searching drug form candidates with prioritized fallback matching.
/// </summary>
internal sealed class SearchDrugFormsHandler(
    IDrugFormRepository _drugFormRepository,
    IVetDrugRepository _vetDrugRepository) : IRequestHandler<SearchDrugFormsQuery, ErrorOr<SearchDrugFormsResponse>>
{
    private static readonly Dictionary<string, DrugFormSearchField> CatalogSearchPriority = new()
    {
        ["commercial_name"] = DrugFormSearchField.CommercialName,
        ["active_substance"] = DrugFormSearchField.ActiveSubstance
    };

    private static readonly Dictionary<string, DrugSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["commercial_name"] = DrugSortField.CommercialName,
        ["created_at"] = DrugSortField.CreatedAt,
        ["updated_at"] = DrugSortField.UpdatedAt
    };

    /// <summary>
    /// Searches drug form candidates and returns a paging envelope with metadata.
    /// </summary>
    /// <param name="query">Search query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items, total count (first page only), and has_more flag.</returns>
    public async Task<ErrorOr<SearchDrugFormsResponse>> Handle(
        SearchDrugFormsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedSearchQuery = NormalizeSearchQuery(query.SearchQuery);
        bool ascending = query.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase);
        DrugSortField sortField = SortFieldMap.GetValueOrDefault(query.SortBy.Trim(), DrugSortField.CommercialName);
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        DrugFormSearchField appliedSearchField = DrugFormSearchField.None;
        string? matchedFieldToken = null;
        List<string> matchedFieldTokens = new();
        List<SearchDrugFormDto> items;

        if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
        {
            List<DrugForm> catalogForms = await _drugFormRepository.SearchFormsAsync(
                searchQuery: null,
                searchField: DrugFormSearchField.None,
                sortField: sortField,
                ascending: ascending,
                skip: query.Skip,
                take: takePlusOne,
                cancellationToken: cancellationToken);

            items = catalogForms
                .Select(SearchDrugFormMappings.MapCatalogFormToDto)
                .ToList();
        }
        else
        {
            items = new List<SearchDrugFormDto>();

            foreach ((string token, DrugFormSearchField field) in CatalogSearchPriority)
            {
                matchedFieldTokens.Add(token);

                List<DrugForm> attempt = await _drugFormRepository.SearchFormsAsync(
                    searchQuery: normalizedSearchQuery,
                    searchField: field,
                    sortField: sortField,
                    ascending: ascending,
                    skip: query.Skip,
                    take: takePlusOne,
                    cancellationToken: cancellationToken);

                if (attempt.Count > 0)
                {
                    items = attempt
                        .Select(SearchDrugFormMappings.MapCatalogFormToDto)
                        .ToList();
                    matchedFieldToken = token;
                    appliedSearchField = field;
                    break;
                }
            }

            if (items.Count == 0)
            {
                matchedFieldTokens.Add("veterinary_commercial_name");

                List<VetDrug> vetAttempt = await _vetDrugRepository.SearchByCommercialNameAsync(
                    searchQuery: normalizedSearchQuery,
                    skip: query.Skip,
                    take: takePlusOne,
                    cancellationToken: cancellationToken);

                if (vetAttempt.Count > 0)
                {
                    items = vetAttempt
                        .Select(SearchDrugFormMappings.MapVetDrugToDto)
                        .ToList();
                    matchedFieldToken = "veterinary_commercial_name";
                    appliedSearchField = DrugFormSearchField.VeterinaryCommercialName;
                }
            }
        }

        bool hasMore = items.Count > query.Take;

        List<SearchDrugFormDto> page = hasMore
            ? items.Take(query.Take).ToList()
            : items;

        if (query.Skip == 0)
        {
            if (page.Count == 0)
            {
                totalCount = 0;
            }
            else if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
            {
                totalCount = await _drugFormRepository.CountFormsAsync(
                    searchQuery: null,
                    searchField: DrugFormSearchField.None,
                    cancellationToken: cancellationToken);
            }
            else if (appliedSearchField == DrugFormSearchField.VeterinaryCommercialName)
            {
                totalCount = await _vetDrugRepository.CountByCommercialNameAsync(
                    searchQuery: normalizedSearchQuery,
                    cancellationToken: cancellationToken);
            }
            else
            {
                totalCount = await _drugFormRepository.CountFormsAsync(
                    searchQuery: normalizedSearchQuery,
                    searchField: appliedSearchField,
                    cancellationToken: cancellationToken);
            }
        }

        SearchDrugFormsMetadata metadata = BuildMetadata(
            normalizedSearchQuery,
            matchedFieldToken,
            page.Count > 0,
            matchedFieldTokens.AsReadOnly());

        return page.MapToResponse(totalCount, hasMore, metadata);
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

    private static SearchDrugFormsMetadata BuildMetadata(
        string? normalizedSearchQuery,
        string? matchedFieldToken,
        bool foundAny,
        IReadOnlyList<string> attemptedFields)
    {
        if (string.IsNullOrWhiteSpace(normalizedSearchQuery))
        {
            return new SearchDrugFormsMetadata(
                MatchedField: null,
                UsedFallback: false,
                AttemptedFields: null,
                Message: "Search executed without query; returning default listing ordered by commercial_name.");
        }

        if (!foundAny)
        {
            string attempted = string.Join(", ", attemptedFields);
            return new SearchDrugFormsMetadata(
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

        return new SearchDrugFormsMetadata(
            MatchedField: matchedFieldToken,
            UsedFallback: usedFallback,
            AttemptedFields: attemptedFields,
            Message: message);
    }
}
