using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

public sealed record HealthcareProviderProfileResponse(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    DateTime? OnboardingCompletedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static HealthcareProviderProfileResponse From(HealthcareProviderProfile profile)
    {
        return new HealthcareProviderProfileResponse(
            profile.ProfessionCode,
            profile.OtherProfessionLabel,
            profile.WorkplaceLabel,
            profile.ShortDescription,
            profile.OnboardingCompletedAtUtc,
            profile.UpdatedAtUtc ?? profile.CreatedAtUtc);
    }
}
