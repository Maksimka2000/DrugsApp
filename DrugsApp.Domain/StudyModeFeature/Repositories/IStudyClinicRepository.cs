using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;

namespace DrugsApp.Domain.StudyModeFeature.Repositories;

public interface IStudyClinicRepository
{
    Task<List<StudyClinicSummary>> SearchSummariesAsync(
        string? search,
        Guid callerUserId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(string? search, Guid callerUserId, CancellationToken cancellationToken);

    Task<bool> IsAccessibleAsync(Guid clinicId, Guid callerUserId, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task<StudyClinic?> GetByIdAsync(Guid clinicId, bool track, CancellationToken cancellationToken);

    Task<StudyClinic?> GetByIdWithLockAsync(Guid clinicId, CancellationToken cancellationToken);

    Task<List<StudyClinic>> ListPrivateByCreatorAsync(Guid creatorUserId, CancellationToken cancellationToken);

    Task<StudyClinicMember?> GetMemberAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken);

    Task<bool> ExistsMemberAsync(
        Guid clinicId,
        Guid userId,
        StudyMemberKind memberKind,
        CancellationToken cancellationToken);

    Task AddAsync(StudyClinic clinic, CancellationToken cancellationToken);

    void Remove(StudyClinic clinic);
}
