using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GetStudyDoctor;

/// <summary>
/// Handles study doctor detail loading from the read repository.
/// </summary>
internal sealed class GetStudyDoctorHandler(IStudyClinicReadRepository _studyClinicReadRepository)
    : IRequestHandler<GetStudyDoctorQuery, ErrorOr<StudyDoctorDetailDto>>
{
    /// <summary>
    /// Loads the doctor member detail read model.
    /// </summary>
    /// <param name="query">Doctor detail query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Doctor details or not found.</returns>
    public async Task<ErrorOr<StudyDoctorDetailDto>> Handle(GetStudyDoctorQuery query, CancellationToken cancellationToken)
    {
        StudyMemberDetailReadModel? detail = await _studyClinicReadRepository.GetMemberDetailAsync(
            query.ClinicId,
            query.DoctorUserId,
            StudyMemberKind.Doctor,
            cancellationToken);
        if (detail is null)
        {
            return StudyModeErrors.MemberNotFound(query.DoctorUserId, "doctorId");
        }

        return detail.MapToDoctorDetailDto();
    }
}
