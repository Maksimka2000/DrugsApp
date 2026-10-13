using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.SearchStudyClinics;

/// <summary>
/// Handles study clinic search by loading the page of summaries with member counts and the total match count.
/// </summary>
internal sealed class SearchStudyClinicsHandler(IStudyClinicRepository _studyClinicRepository)
    : IRequestHandler<SearchStudyClinicsQuery, ErrorOr<StudyClinicListResponse>>
{
    /// <summary>
    /// Loads the requested page of study clinics and the total count for the same filter.
    /// </summary>
    /// <param name="query">Search query with filter and paging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged study clinic envelope.</returns>
    public async Task<ErrorOr<StudyClinicListResponse>> Handle(
        SearchStudyClinicsQuery query,
        CancellationToken cancellationToken)
    {
        List<StudyClinicSummary> summaries = await _studyClinicRepository.SearchSummariesAsync(
            search: query.Request.Search,
            callerUserId: query.CallerUserId,
            skip: query.Request.Skip,
            take: query.Request.Take,
            cancellationToken: cancellationToken);

        int totalCount = await _studyClinicRepository.CountAsync(
            query.Request.Search,
            query.CallerUserId,
            cancellationToken);

        return summaries.MapToResponse(totalCount, query.CallerUserId);
    }
}
