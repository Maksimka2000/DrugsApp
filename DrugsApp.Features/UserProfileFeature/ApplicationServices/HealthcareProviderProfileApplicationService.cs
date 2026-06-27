using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service for onboarding and updating healthcare provider profiles.
/// </summary>
internal sealed class HealthcareProviderProfileApplicationService(
    IUserProfileRepository _userProfileRepository,
    IHealthcareProviderProfileRepository _healthcareProviderProfileRepository)
    : IHealthcareProviderProfileApplicationService
{
    /// <summary>
    /// Creates or updates a provider profile during onboarding and marks completion when provided.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="request">Profile upsert payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Upsert result or errors.</returns>
    public async Task<ErrorOr<HealthcareProviderProfileUpsertResult>> UpsertFromOnboardingAsync(
        Guid userId,
        HealthcareProviderProfileUpsertRequest request,
        CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userProfile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = userId.ToString()
                });
        }

        HealthcareProviderProfile? existing = await _healthcareProviderProfileRepository.GetTrackedByUserIdAsync(userId, cancellationToken);

        if (existing is null)
        {
            ErrorOr<HealthcareProviderProfile> created = HealthcareProviderProfile.Create(
                userId,
                request.ProfessionCode,
                request.OtherProfessionLabel,
                request.WorkplaceLabel,
                request.ShortDescription);

            if (created.IsError)
            {
                return created.Errors;
            }

            if (request.OnboardingCompletedAtUtc is not null)
            {
                created.Value.MarkOnboardingCompleted(request.OnboardingCompletedAtUtc.Value);
            }

            await _healthcareProviderProfileRepository.AddAsync(created.Value, cancellationToken);

            return new HealthcareProviderProfileUpsertResult(created.Value, true);
        }

        ErrorOr<Success> updateResult = existing.Update(
            request.ProfessionCode,
            request.OtherProfessionLabel,
            request.WorkplaceLabel,
            request.ShortDescription);

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        if (request.OnboardingCompletedAtUtc is not null)
        {
            existing.MarkOnboardingCompleted(request.OnboardingCompletedAtUtc.Value);
        }

        return new HealthcareProviderProfileUpsertResult(existing, false);
    }
}
