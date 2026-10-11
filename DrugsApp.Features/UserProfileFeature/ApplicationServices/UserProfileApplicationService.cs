using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service creating completed user profiles and applying personal-data updates, including the
/// reproductive health profile cleanup when gender stops being female.
/// </summary>
internal sealed class UserProfileApplicationService(
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository)
    : IUserProfileApplicationService
{
    /// <summary>
    /// Creates a user profile already marked as onboarding-completed and stages it for insert.
    /// </summary>
    /// <param name="userId">
    /// Identity user identifier shared with the profile.
    /// </param>
    /// <param name="email">
    /// Active login email.
    /// </param>
    /// <param name="personalData">
    /// Personal data of the profile.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or validation errors.
    /// </returns>
    public async Task<ErrorOr<Success>> CreateCompletedProfileAsync(
        Guid userId,
        string email,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken)
    {
        ErrorOr<UserProfile> profileResult = UserProfile.Create(
            userId,
            email,
            personalData.FirstName,
            personalData.LastName,
            personalData.DateOfBirth,
            personalData.Gender);
        if (profileResult.IsError)
        {
            return profileResult.Errors;
        }

        profileResult.Value.MarkOnboardingCompleted(DateTime.UtcNow);

        await _userProfileRepository.AddAsync(profileResult.Value, cancellationToken);

        return Result.Success;
    }

    /// <summary>
    /// Applies new personal data to the tracked profile and removes the reproductive health profile when the new
    /// gender is not female.
    /// </summary>
    /// <param name="userId">
    /// Owner user identifier.
    /// </param>
    /// <param name="personalData">
    /// New personal data.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Update outcome, not found, or validation errors.
    /// </returns>
    public async Task<ErrorOr<UserProfilePersonalDataUpdateResult>> UpdatePersonalDataAsync(
        Guid userId,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await _userProfileRepository.GetTrackedByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Error.NotFound(
                code: "UserProfile.NotFound",
                description: "User profile not found.",
                metadata: new()
                {
                    ["field"] = "userId",
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = userId.ToString()
                });
        }

        ErrorOr<Success> updateResult = profile.UpdatePersonalData(
            personalData.FirstName,
            personalData.LastName,
            personalData.DateOfBirth,
            personalData.Gender);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        bool reproductiveHealthProfileRemoved = false;
        if (personalData.Gender != ProfileGender.female
            && await _clientReproductiveHealthProfileRepository.ExistsByUserIdAsync(userId, cancellationToken))
        {
            await _clientReproductiveHealthProfileRepository.RemoveByUserIdAsync(userId, cancellationToken);
            reproductiveHealthProfileRemoved = true;
        }

        return new UserProfilePersonalDataUpdateResult(reproductiveHealthProfileRemoved);
    }
}
