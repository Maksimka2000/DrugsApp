using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.UserProfileFeature.Shared;

/// <summary>
/// Response representing a healthcare provider profile.
/// </summary>
/// <param name="ProfessionCode">Provider profession code.</param>
/// <param name="OtherProfessionLabel">Custom profession label.</param>
/// <param name="WorkplaceLabel">Workplace description.</param>
/// <param name="ShortDescription">Provider description.</param>
/// <param name="OnboardingCompletedAtUtc">Onboarding completion timestamp.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
public sealed record HealthcareProviderProfileResponse(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    DateTime? OnboardingCompletedAtUtc,
    DateTime UpdatedAtUtc)
{
    /// <summary>
    /// Maps a provider profile entity to response.
    /// </summary>
    /// <param name="profile">Source profile entity.</param>
    /// <returns>Response DTO.</returns>
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
