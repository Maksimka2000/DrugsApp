using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.SearchTempDrugs;

/// <summary>
/// Handles searching temporary drugs with filters and paging metadata.
/// </summary>
internal sealed class SearchTempDrugsHandler
    : IRequestHandler<SearchTempDrugsQuery, ErrorOr<SearchTempDrugsResponse>>
{
    private static readonly Dictionary<string, TempDrugSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["created_at"] = TempDrugSortField.CreatedAtUtc,
        ["updated_at"] = TempDrugSortField.UpdatedAtUtc,
        ["commercial_name"] = TempDrugSortField.CommercialName,
        ["identifier"] = TempDrugSortField.ProductIdentifier
    };

    private static readonly Dictionary<string, TempDrugStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pending_review"] = TempDrugStatus.PendingReview,
        ["review_later"] = TempDrugStatus.ReviewLater,
        ["reviewed_resolved"] = TempDrugStatus.ReviewedResolved,
        ["reviewed_added"] = TempDrugStatus.ReviewedAdded
    };

    private const string HumanTypeOfProduct = "Ludzki";
    private const string VeterinaryTypeOfProduct = "Weterynaryjny";

    private static readonly Dictionary<string, string> TypeOfProductsMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ludzki"] = HumanTypeOfProduct,
        ["weterynaryjny"] = VeterinaryTypeOfProduct
    };

    private readonly ITempDrugRepository _tempDrugRepository;

    public SearchTempDrugsHandler(ITempDrugRepository tempDrugRepository)
    {
        _tempDrugRepository = tempDrugRepository;
    }

    /// <summary>
    /// Executes the temporary drugs search and returns the paging envelope.
    /// </summary>
    /// <param name="query">Search query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items, counts (first page only), and has_more flag.</returns>
    public async Task<ErrorOr<SearchTempDrugsResponse>> Handle(
        SearchTempDrugsQuery query,
        CancellationToken cancellationToken)
    {
        // initialize normalized name filters
        string? normalizedCommercialName = NormalizeCommercialName(query.CommercialName);
        // normalize statuses filter
        IReadOnlyCollection<TempDrugStatus>? normalizedStatusesFilter = NormalizeStatusesFilter(query.Statuses);
        // normalize type of products filter
        IReadOnlyCollection<string>? normalizedTypeOfProductsFilter = NormalizeTypeOfProductsFilter(query.TypeOfProducts);
        // determine sort order
        bool ascending = query.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase);
        // get sort field
        TempDrugSortField sortField = SortFieldMap.GetValueOrDefault(query.SortBy.Trim(), TempDrugSortField.CreatedAtUtc);
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // search temp drugs
        List<TempDrug> tempDrugs = await _tempDrugRepository.SearchAsync(
            normalizedCommercialName,
            normalizedStatusesFilter,
            normalizedTypeOfProductsFilter,
            sortField,
            ascending,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = tempDrugs.Count > query.Take;

        // get page
        List<TempDrug> page = hasMore
            ? tempDrugs.Take(query.Take).ToList()
            : tempDrugs;

        // map to DTOs
        List<TempDrugDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            totalCount = await _tempDrugRepository.CountAsync(
                normalizedCommercialName,
                normalizedStatusesFilter,
                normalizedTypeOfProductsFilter,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }


    private static string? NormalizeCommercialName(string? commercialName)
    {
        if (string.IsNullOrWhiteSpace(commercialName))
        {
            return null;
        }

        return commercialName.Trim();
    }

    private static IReadOnlyCollection<TempDrugStatus>? NormalizeStatusesFilter(List<string>? statuses)
    {
        if (statuses is null || statuses.Count == 0)
        {
            return null;
        }

        IReadOnlyCollection<TempDrugStatus> distinct = statuses
            .Select(status => StatusMap.TryGetValue(status, out TempDrugStatus mapped) ? mapped : (TempDrugStatus?)null)
            .Where(mapped => mapped.HasValue)
            .Select(mapped => mapped!.Value)
            .Distinct()
            .ToList()
            .AsReadOnly();

        if (distinct.Count == 0)
        {
            return null;
        }

        var all = Enum.GetValues<TempDrugStatus>().ToHashSet();

        bool selectsAll = all.SetEquals(distinct);

        return selectsAll ? null : distinct;
    }

    private static IReadOnlyCollection<string>? NormalizeTypeOfProductsFilter(List<string>? typeOfProducts)
    {
        if (typeOfProducts is null || typeOfProducts.Count == 0)
        {
            return null;
        }

        IReadOnlyCollection<string?> distinct = typeOfProducts
            .Select(product => !string.IsNullOrWhiteSpace(product) && TypeOfProductsMap.TryGetValue(product, out string? mapped) ? mapped : null)
            .Where(mapped => !string.IsNullOrWhiteSpace(mapped))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            .AsReadOnly();

        if (distinct.Count == 0)
        {
            return null;
        }

        var all = TypeOfProductsMap.Values.ToHashSet();

        bool selectsAll = all.SetEquals(distinct!);

        return selectsAll ? null : distinct;
    }
}
