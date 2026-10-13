using DrugsApp.Domain.StudyModeFeature.DomainServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomProfile;

/// <summary>
/// Handles the random patient profile suggestion; pure generation, no persistence.
/// </summary>
internal sealed class GenerateStudyPatientRandomProfileHandler
    : IRequestHandler<GenerateStudyPatientRandomProfileQuery, ErrorOr<StudyRandomPatientProfileResponse>>
{
    /// <summary>
    /// Generates a random profile relative to today's UTC date.
    /// </summary>
    /// <param name="query">Suggestion query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The suggested profile.</returns>
    public Task<ErrorOr<StudyRandomPatientProfileResponse>> Handle(
        GenerateStudyPatientRandomProfileQuery query,
        CancellationToken cancellationToken)
    {
        DateOnly todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        StudyRandomPersonProfile profile = StudyRandomPersonGenerator.GeneratePatientProfile(todayUtc);

        ErrorOr<StudyRandomPatientProfileResponse> result = profile.MapToResponse();
        return Task.FromResult(result);
    }
}
