using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using MediatR;
using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Features.SupplementFeature.SearchSupplements;


/// <summary>
/// Handles supplement search by name/aliases with pagination.
/// </summary>
public sealed class SearchSupplementsHandler(ISupplementRepository _supplementRepository) : IRequestHandler<SearchSupplementsQuery, ErrorOr<SearchSupplementsResponse>>
{

    public async Task<ErrorOr<SearchSupplementsResponse>> Handle(
        SearchSupplementsQuery query,
        CancellationToken cancellationToken)
    {
        // normalize supplement name query
        string? normalizedSupplementNameQuery = string.IsNullOrWhiteSpace(query.SupplementName)
            ? null
            : query.SupplementName!.Trim();
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // initialize search result list
        List<Supplement> rows = new();

        // search supplements
        rows = await _supplementRepository.SearchAsync(
            query.SupplementName,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = rows.Count > query.Take;
        // get page
        var page = rows.Take(query.Take).ToList();

        // count total items
        if (query.Skip == 0)
        {
            if (rows.Count == 0)
            {
                totalCount = 0;
            }
            else
            {
                // count total items
                totalCount = await _supplementRepository.CountAsync(
                    query.SupplementName,
                    cancellationToken);
            }
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
