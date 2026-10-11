using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomData;

/// <summary>
/// Handles random sample data generation; the generated rows are committed together by the outer
/// <c>TransactionBehavior</c> transaction.
/// </summary>
internal sealed class GenerateStudyPatientRandomDataHandler(
    IStudyRandomMedicalDataApplicationService _studyRandomMedicalDataApplicationService)
    : IRequestHandler<GenerateStudyPatientRandomDataCommand, ErrorOr<StudyRandomDataResponse>>
{
    /// <summary>
    /// Generates the requested items and reports how many were created.
    /// </summary>
    /// <param name="command">Generation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation counts, the limit conflict when nothing could be added, or unexpected errors.</returns>
    public async Task<ErrorOr<StudyRandomDataResponse>> Handle(
        GenerateStudyPatientRandomDataCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<StudyRandomDataResult> result = await _studyRandomMedicalDataApplicationService.GenerateAsync(
            command.PatientUserId,
            command.Section,
            command.Count,
            cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        bool isLimitReachedBeforeAnyItem = result.Value.CreatedCount == 0
            && result.Value.LimitReached
            && result.Value.LimitErrors is { Count: > 0 };
        if (isLimitReachedBeforeAnyItem)
        {
            return result.Value.LimitErrors!;
        }

        return result.Value.MapToResponse();
    }
}
