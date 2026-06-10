using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

internal sealed class HealthcareProviderTeardownService(
    IHealthcareProviderProfileRepository _healthcareProviderProfileRepository,
    IIdentityAuthenticationService _identityAuthenticationService)
    : IHealthcareProviderTeardownService
{
    public async Task<ErrorOr<HealthcareProviderTeardownResult>> RemoveProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? providerProfile = await _healthcareProviderProfileRepository.GetTrackedByUserIdAsync(userId, cancellationToken);
        if (providerProfile is null)
        {
            return Error.NotFound(
                description: "Healthcare provider profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "HealthcareProviderProfile",
                    ["resourceId"] = userId.ToString()
                });
        }

        await _healthcareProviderProfileRepository.RemoveByUserIdAsync(userId, cancellationToken);

        ErrorOr<Success> roleResult = await _identityAuthenticationService.RemoveHealthcareProviderRoleAsync(
            userId,
            cancellationToken);

        if (roleResult.IsError)
        {
            return roleResult.Errors;
        }

        return new HealthcareProviderTeardownResult(
            RemovedPatientCount: 0,
            RemovedPendingRequestCount: 0,
            RemovedProposedChangeCount: 0,
            IsHealthcareProvider: false);
    }
}
