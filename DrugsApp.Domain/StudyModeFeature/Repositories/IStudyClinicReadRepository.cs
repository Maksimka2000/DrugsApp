using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;

namespace DrugsApp.Domain.StudyModeFeature.Repositories;

public interface IStudyClinicReadRepository
{
    Task<StudyClinicCanvasReadModel?> GetCanvasAsync(Guid clinicId, CancellationToken cancellationToken);

    Task<StudyMemberDetailReadModel?> GetMemberDetailAsync(
        Guid clinicId,
        Guid userId,
        StudyMemberKind memberKind,
        CancellationToken cancellationToken);
}
