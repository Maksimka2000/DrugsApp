using System;

namespace DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;

internal static class AddHealthcareProviderProfileMapping
{
    public static AddHealthcareProviderProfileCommand MapToCommand(
        this AddHealthcareProviderProfileRequest request,
        Guid userId) =>
        new(userId, request);
}
