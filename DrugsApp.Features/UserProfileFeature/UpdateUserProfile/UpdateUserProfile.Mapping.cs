using System;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.Shared;

namespace DrugsApp.Features.UserProfileFeature.UpdateUserProfile;

internal static class UpdateUserProfileMapping
{
    public static UpdateUserProfileCommand MapToCommand(this UpdateUserProfileRequest request, Guid userId, string email)
    {
        return new UpdateUserProfileCommand(
            userId,
            email,
            request with
            {
                Gender = request.Gender.Trim().ToLowerInvariant()
            });
    }

    public static ProfileGender ToGenderEnum(this string gender) =>
        Enum.Parse<ProfileGender>(gender, ignoreCase: true);
}
