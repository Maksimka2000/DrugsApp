using DrugsApp.Domain.StudyModeFeature.Entities;
using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Deletes study accounts together with every piece of data they own; shared by clinic deletion and single
/// account deletion.
/// </summary>
public interface IStudyAccountTeardownApplicationService
{
    /// <summary>
    /// Tears down every member account of the tracked clinic.
    /// </summary>
    /// <param name="clinic">
    /// Tracked clinic aggregate loaded with its members.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or the first teardown error.
    /// </returns>
    Task<ErrorOr<Success>> TeardownAllMembersAsync(StudyClinic clinic, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the member from the clinic and deletes the study account and all its owned data.
    /// </summary>
    /// <param name="clinic">
    /// Tracked clinic aggregate loaded with its members.
    /// </param>
    /// <param name="userId">
    /// Study account user identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success, not found when the user is not a member, or account teardown errors.
    /// </returns>
    Task<ErrorOr<Success>> TeardownMemberAsync(StudyClinic clinic, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes every private clinic created by the user together with all their study accounts; public clinics
    /// of the user are kept.
    /// </summary>
    /// <param name="creatorUserId">
    /// Clinic creator (teacher or admin) being removed.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or the first teardown error.
    /// </returns>
    Task<ErrorOr<Success>> DeletePrivateClinicsOfCreatorAsync(Guid creatorUserId, CancellationToken cancellationToken);
}
