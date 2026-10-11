using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.SearchStudyClinics;

/// <summary>
/// Pure mapping helpers of SearchStudyClinics (no I/O, no business rules).
/// </summary>
public static class SearchStudyClinicsMapping
{
    /// <summary>
    /// Maps the HTTP request to the MediatR query.
    /// </summary>
    public static SearchStudyClinicsQuery MapToQuery(this SearchStudyClinicsRequest request, Guid callerUserId) => new(
        Request: request,
        CallerUserId: callerUserId);

    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static StudyClinicListResponse MapToResponse(this List<StudyClinicSummary> summaries, int totalCount, Guid callerUserId) => new(
        Items: summaries.Select(summary => summary.MapToDto(callerUserId)).ToList(),
        TotalCount: totalCount);
}
