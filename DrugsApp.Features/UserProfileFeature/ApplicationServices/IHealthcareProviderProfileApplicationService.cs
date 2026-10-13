using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service contract for healthcare provider profile onboarding/upsert workflows.
/// </summary>
public interface IHealthcareProviderProfileApplicationService
{
    /// <summary>
    /// Upserts provider profile data during onboarding.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="request">Profile upsert payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Upsert result or errors.</returns>
    Task<ErrorOr<HealthcareProviderProfileUpsertResult>> UpsertFromOnboardingAsync(
        Guid userId,
        HealthcareProviderProfileUpsertRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a provider profile already marked as onboarding-completed without reading the user profile, which
    /// may only be staged in the current unit of work (study account provisioning).
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="request">Provider profile payload; a null completion timestamp defaults to the current UTC time.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Success or validation errors.</returns>
    Task<ErrorOr<Success>> CreateCompletedAsync(
        Guid userId,
        HealthcareProviderProfileUpsertRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Request payload for healthcare provider profile upsert operations.
/// </summary>
/// <param name="ProfessionCode">Optional profession code.</param>
/// <param name="OtherProfessionLabel">Custom profession label when profession is other.</param>
/// <param name="WorkplaceLabel">Optional workplace label.</param>
/// <param name="ShortDescription">Optional provider description.</param>
/// <param name="OnboardingCompletedAtUtc">Optional onboarding completion timestamp.</param>
public sealed record HealthcareProviderProfileUpsertRequest(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    DateTime? OnboardingCompletedAtUtc);

/// <summary>
/// Result of a healthcare provider profile upsert.
/// </summary>
/// <param name="Profile">The resulting profile aggregate.</param>
/// <param name="Created">Indicates whether the profile was newly created.</param>
public sealed record HealthcareProviderProfileUpsertResult(
    HealthcareProviderProfile Profile,
    bool Created);
