using System;

namespace DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;

internal static class UpdateReproductiveHealthProfileMapping
{
    public static UpdateReproductiveHealthProfileCommand MapToCommand(
        this UpdateReproductiveHealthProfileRequest request,
        Guid userId) =>
        new(userId, request);
}
