using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service contract for user profile creation and personal-data updates shared by the client profile
/// flow and study mode account provisioning.
/// </summary>
public interface IUserProfileApplicationService
{
    /// <summary>
    /// Creates a user profile that is already marked as onboarding-completed.
    /// </summary>
    /// <param name="userId">
    /// Identity user identifier shared with the profile.
    /// </param>
    /// <param name="email">
    /// Active login email mirrored into the profile.
    /// </param>
    /// <param name="personalData">
    /// First name, last name, date of birth and gender.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or validation errors raised by the profile factory.
    /// </returns>
    Task<ErrorOr<Success>> CreateCompletedProfileAsync(
        Guid userId,
        string email,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates name, date of birth and gender, and removes the reproductive health profile when the gender is
    /// no longer female.
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
    /// Update outcome, not found when the profile is missing, or validation errors.
    /// </returns>
    Task<ErrorOr<UserProfilePersonalDataUpdateResult>> UpdatePersonalDataAsync(
        Guid userId,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken);
}

/// <summary>
/// Outcome of a personal-data update.
/// </summary>
/// <param name="ReproductiveHealthProfileRemoved">Whether the reproductive health profile was removed.</param>
public sealed record UserProfilePersonalDataUpdateResult(
    bool ReproductiveHealthProfileRemoved);
