using System;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

internal static class MemberProfilePhotoUrls
{
    public static string? Build(Guid userId, string? profilePhotoStorageKey) =>
        string.IsNullOrWhiteSpace(profilePhotoStorageKey)
            ? null
            : $"/api/user-profiles/{userId}/photo";
}
