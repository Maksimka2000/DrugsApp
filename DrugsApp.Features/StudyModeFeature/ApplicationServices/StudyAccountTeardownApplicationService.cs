using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Removes study members from their clinic and reuses the client account teardown, which deletes medical data,
/// supervision rows, profiles, tokens, the profile photo blob and finally the Identity user.
/// </summary>
internal sealed class StudyAccountTeardownApplicationService(
    IIdentityAuthenticationService _identityAuthenticationService,
    IStudyClinicRepository _studyClinicRepository)
    : IStudyAccountTeardownApplicationService
{
    /// <summary>
    /// Tears down every member account of the tracked clinic, iterating over a snapshot of member ids.
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
    public async Task<ErrorOr<Success>> TeardownAllMembersAsync(StudyClinic clinic, CancellationToken cancellationToken)
    {
        List<Guid> memberUserIds = clinic.Members
            .Select(member => member.UserId)
            .ToList();

        foreach (Guid memberUserId in memberUserIds)
        {
            ErrorOr<Success> teardownResult = await TeardownMemberAsync(clinic, memberUserId, cancellationToken);
            if (teardownResult.IsError)
            {
                return teardownResult.Errors;
            }
        }

        return Result.Success;
    }

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
    /// Success or errors.
    /// </returns>
    public async Task<ErrorOr<Success>> TeardownMemberAsync(
        StudyClinic clinic,
        Guid userId,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> removeResult = clinic.RemoveMember(userId);
        if (removeResult.IsError)
        {
            return removeResult.Errors;
        }

        return await _identityAuthenticationService.TeardownClientAccountAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Deletes every private clinic created by the user together with all their study accounts.
    /// </summary>
    /// <param name="creatorUserId">
    /// Clinic creator being removed.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or the first teardown error.
    /// </returns>
    public async Task<ErrorOr<Success>> DeletePrivateClinicsOfCreatorAsync(
        Guid creatorUserId,
        CancellationToken cancellationToken)
    {
        List<StudyClinic> privateClinics = await _studyClinicRepository.ListPrivateByCreatorAsync(
            creatorUserId,
            cancellationToken);

        foreach (StudyClinic clinic in privateClinics)
        {
            ErrorOr<Success> teardownResult = await TeardownAllMembersAsync(clinic, cancellationToken);
            if (teardownResult.IsError)
            {
                return teardownResult.Errors;
            }

            _studyClinicRepository.Remove(clinic);
        }

        return Result.Success;
    }
}
