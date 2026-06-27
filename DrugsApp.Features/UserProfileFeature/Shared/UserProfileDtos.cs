using System;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

/// <summary>
/// Response DTO representing a user's profile.
/// </summary>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="DateOfBirth">Date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="Email">Primary email address.</param>
/// <param name="HasCustomProfilePhoto">Indicates whether a custom profile photo exists.</param>
/// <param name="ProfilePhotoUrl">URL to fetch the profile photo.</param>
/// <param name="HasReproductiveHealthProfile">Whether reproductive health profile exists.</param>
/// <param name="PendingLoginEmail">Pending login email if email change is in progress.</param>
/// <param name="OnboardingCompletedAtUtc">Onboarding completion timestamp, if any.</param>
/// <param name="ShowMedicalReviewHint">Flag to show medical review hint after gender change.</param>
public sealed record UserProfileResponse(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileGender Gender,
    string Email,
    bool HasCustomProfilePhoto,
    string? ProfilePhotoUrl,
    bool HasReproductiveHealthProfile,
    string? PendingLoginEmail,
    DateTime? OnboardingCompletedAtUtc,
    bool? ShowMedicalReviewHint = null)
{
    /// <summary>
    /// Maps a profile aggregate to the response DTO.
    /// </summary>
    /// <param name="profile">User profile aggregate.</param>
    /// <param name="hasReproductiveHealthProfile">Whether reproductive profile exists.</param>
    /// <param name="pendingLoginEmail">Pending login email.</param>
    /// <param name="profilePhotoUrl">Profile photo URL.</param>
    /// <param name="showMedicalReviewHint">Optional flag to show review hint.</param>
    /// <returns>Response DTO.</returns>
    public static UserProfileResponse From(
        UserProfile profile,
        bool hasReproductiveHealthProfile,
        string? pendingLoginEmail,
        string? profilePhotoUrl,
        bool? showMedicalReviewHint = null)
    {
        return new UserProfileResponse(
            FirstName: profile.FirstName,
            LastName: profile.LastName,
            DateOfBirth: profile.DateOfBirth,
            Gender: profile.Gender,
            Email: profile.Email,
            HasCustomProfilePhoto: profile.ProfilePhotoStorageKey is not null,
            ProfilePhotoUrl: profilePhotoUrl,
            HasReproductiveHealthProfile: hasReproductiveHealthProfile,
            PendingLoginEmail: pendingLoginEmail,
            OnboardingCompletedAtUtc: profile.OnboardingCompletedAtUtc,
            ShowMedicalReviewHint: showMedicalReviewHint);
    }
}

/// <summary>
/// Response DTO with profile photo URL.
/// </summary>
/// <param name="ProfilePhotoUrl">Download URL for the profile photo.</param>
public sealed record UserProfilePhotoResponse(string ProfilePhotoUrl);
