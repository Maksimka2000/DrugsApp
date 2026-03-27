using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.SearchImportBatches;

/// <summary>
/// Handles paged search of import batches history.
/// </summary>
internal sealed class SearchImportBatchesHandler(ITempDrugImportBatchRepository _batchRepository)
    : IRequestHandler<SearchImportBatchesQuery, ErrorOr<SearchImportBatchesResponse>>
{

    private static readonly Dictionary<string, TempDrugImportBatchSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["created_at"] = TempDrugImportBatchSortField.CreatedAtUtc,
    };

    public async Task<ErrorOr<SearchImportBatchesResponse>> Handle(
        SearchImportBatchesQuery query,
        CancellationToken cancellationToken)
    {
        // determine sort order
        bool ascending = query.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase);
        // get sort field
        TempDrugImportBatchSortField sortField = SortFieldMap.GetValueOrDefault(query.SortBy.Trim(), TempDrugImportBatchSortField.CreatedAtUtc);
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // search temp drugs
        List<TempDrugImportBatch> batches = await _batchRepository.SearchAsync(
            sortField: sortField,
            ascending: ascending,
            skip: query.Skip,
            take: takePlusOne,
            cancellationToken: cancellationToken);

        // determine if there are more items
        bool hasMore = batches.Count > query.Take;

        // get page
        List<TempDrugImportBatch> page = hasMore ? batches.Take(query.Take).ToList() : batches;
        List<ImportBatchDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            totalCount = await _batchRepository.CountAsync(cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}

