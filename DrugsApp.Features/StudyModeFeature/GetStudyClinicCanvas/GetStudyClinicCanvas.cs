using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GetStudyClinicCanvas;

/// <summary>
/// Handles canvas loading from the study clinic read repository.
/// </summary>
internal sealed class GetStudyClinicCanvasHandler(IStudyClinicReadRepository _studyClinicReadRepository)
    : IRequestHandler<GetStudyClinicCanvasQuery, ErrorOr<StudyClinicCanvasResponse>>
{
    /// <summary>
    /// Loads the canvas read model and maps it with deterministic ordering.
    /// </summary>
    /// <param name="query">Canvas query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canvas response or not found.</returns>
    public async Task<ErrorOr<StudyClinicCanvasResponse>> Handle(
        GetStudyClinicCanvasQuery query,
        CancellationToken cancellationToken)
    {
        StudyClinicCanvasReadModel? canvas = await _studyClinicReadRepository.GetCanvasAsync(
            query.ClinicId,
            cancellationToken);
        if (canvas is null)
        {
            return StudyModeErrors.ClinicNotFound(query.ClinicId);
        }

        return canvas.MapToResponse(query.CallerUserId);
    }
}
