using System;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

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
    bool? ShowMedicalReviewHint = null)
{
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
            ShowMedicalReviewHint: showMedicalReviewHint);
    }
}

public sealed record UserProfilePhotoResponse(string ProfilePhotoUrl);
