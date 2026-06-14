using System;

namespace DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;

internal static class AddReproductiveHealthProfileMapping
{
    public static AddReproductiveHealthProfileCommand MapToCommand(
        this AddReproductiveHealthProfileRequest request,
        Guid userId) =>
        new(userId, request);
}
