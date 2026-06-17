using System;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

/// <summary>
/// 
/// </summary>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="DateOfBirth"></param>
/// <param name="Gender"></param>
/// <param name="Email"></param>
/// <param name="HasCustomProfilePhoto"></param>
/// <param name="ProfilePhotoUrl"></param>
/// <param name="HasReproductiveHealthProfile"></param>
/// <param name="PendingLoginEmail"></param>
/// <param name="OnboardingCompletedAtUtc"></param>
/// <param name="ShowMedicalReviewHint"></param>
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
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="hasReproductiveHealthProfile"></param>
    /// <param name="pendingLoginEmail"></param>
    /// <param name="profilePhotoUrl"></param>
    /// <param name="showMedicalReviewHint"></param>
    /// <returns></returns>
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
/// 
/// </summary>
/// <param name="ProfilePhotoUrl"></param>
public sealed record UserProfilePhotoResponse(string ProfilePhotoUrl);
