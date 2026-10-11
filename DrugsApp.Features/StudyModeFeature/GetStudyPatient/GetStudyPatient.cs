using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GetStudyPatient;

/// <summary>
/// Handles study patient detail loading from the read repository.
/// </summary>
internal sealed class GetStudyPatientHandler(IStudyClinicReadRepository _studyClinicReadRepository)
    : IRequestHandler<GetStudyPatientQuery, ErrorOr<StudyPatientDetailDto>>
{
    /// <summary>
    /// Loads the patient member detail read model including medical counters.
    /// </summary>
    /// <param name="query">Patient detail query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Patient details or not found.</returns>
    public async Task<ErrorOr<StudyPatientDetailDto>> Handle(GetStudyPatientQuery query, CancellationToken cancellationToken)
    {
        StudyMemberDetailReadModel? detail = await _studyClinicReadRepository.GetMemberDetailAsync(
            query.ClinicId,
            query.PatientUserId,
            StudyMemberKind.Patient,
            cancellationToken);
        if (detail is null)
        {
            return StudyModeErrors.MemberNotFound(query.PatientUserId, "patientId");
        }

        return detail.MapToPatientDetailDto();
    }
}
