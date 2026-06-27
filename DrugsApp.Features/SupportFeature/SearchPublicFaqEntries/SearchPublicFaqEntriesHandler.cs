using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupportFeature.SearchPublicFaqEntries;

/// <summary>
/// Handles searching public FAQ entries with pagination metadata.
/// </summary>
internal sealed class SearchPublicFaqEntriesHandler(IFaqEntryRepository _faqEntryRepository)
    : IRequestHandler<SearchPublicFaqEntriesQuery, ErrorOr<SearchPublicFaqEntriesResponse>>
{
    /// <summary>
    /// Searches public FAQ entries using the support repository.
    /// </summary>
    /// <param name="query">Search query containing language, optional filter, and paging data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Public FAQ search response with page items and metadata.</returns>
    public async Task<ErrorOr<SearchPublicFaqEntriesResponse>> Handle(
        SearchPublicFaqEntriesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        List<FaqEntry> faqEntries = await _faqEntryRepository.SearchPublicAsync(
            languageCode: query.LanguageCode,
            questionQuery: query.QuestionQuery,
            skip: query.Skip,
            take: takePlusOne,
            cancellationToken: cancellationToken);

        bool hasMore = faqEntries.Count > query.Take;
        List<FaqEntry> page = hasMore
            ? faqEntries.Take(query.Take).ToList()
            : faqEntries;

        IReadOnlyCollection<PublicFaqEntryDto> items = page.MapToPublicDtoList(query.LanguageCode);

        if (query.Skip == 0)
        {
            totalCount = faqEntries.Count == 0
                ? 0
                : await _faqEntryRepository.CountPublicAsync(
                    languageCode: query.LanguageCode,
                    questionQuery: query.QuestionQuery,
                    cancellationToken: cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}
