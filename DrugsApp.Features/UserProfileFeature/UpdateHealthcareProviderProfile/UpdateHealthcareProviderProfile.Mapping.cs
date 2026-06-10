using System;

namespace DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;

internal static class UpdateHealthcareProviderProfileMapping
{
    public static UpdateHealthcareProviderProfileCommand MapToCommand(
        this UpdateHealthcareProviderProfileRequest request,
        Guid userId) =>
        new(userId, request);
}
