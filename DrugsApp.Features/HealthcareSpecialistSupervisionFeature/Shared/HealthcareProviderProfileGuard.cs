using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

internal static class HealthcareProviderProfileGuard
{
    public static async Task<ErrorOr<Success>> EnsureExistsAsync(
        IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
        Guid providerUserId,
        CancellationToken cancellationToken)
    {
        if (await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(providerUserId, cancellationToken) is null)
        {
            return Error.Forbidden(description: "Provider profile required.");
        }

        return Result.Success;
    }
}
