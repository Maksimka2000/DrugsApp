using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupportFeature.SearchAdminFaqEntries;

/// <summary>
/// Handles searching admin FAQ entries with pagination metadata.
/// </summary>
internal sealed class SearchAdminFaqEntriesHandler(IFaqEntryRepository _faqEntryRepository)
    : IRequestHandler<SearchAdminFaqEntriesQuery, ErrorOr<SearchAdminFaqEntriesResponse>>
{
    /// <summary>
    /// Searches admin FAQ entries using the support repository.
    /// </summary>
    /// <param name="query">Search query containing optional filter and paging data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Admin FAQ search response with page items and metadata.</returns>
    public async Task<ErrorOr<SearchAdminFaqEntriesResponse>> Handle(
        SearchAdminFaqEntriesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        List<FaqEntry> faqEntries = await _faqEntryRepository.SearchAdminAsync(
            questionQuery: query.QuestionQuery,
            skip: query.Skip,
            take: takePlusOne,
            cancellationToken: cancellationToken);

        bool hasMore = faqEntries.Count > query.Take;
        List<FaqEntry> page = hasMore
            ? faqEntries.Take(query.Take).ToList()
            : faqEntries;

        IReadOnlyCollection<AdminFaqEntryDto> items = page.MapToAdminDtoList();

        if (query.Skip == 0)
        {
            totalCount = faqEntries.Count == 0
                ? 0
                : await _faqEntryRepository.CountAdminAsync(
                    questionQuery: query.QuestionQuery,
                    cancellationToken: cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}
