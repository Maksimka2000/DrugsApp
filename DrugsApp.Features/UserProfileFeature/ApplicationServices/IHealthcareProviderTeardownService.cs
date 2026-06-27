using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service contract for removing healthcare provider role and related artifacts.
/// </summary>
public interface IHealthcareProviderTeardownService
{
    /// <summary>
    /// Removes provider role and related data for a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Teardown result or errors.</returns>
    Task<ErrorOr<HealthcareProviderTeardownResult>> RemoveProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Result of provider role teardown.
/// </summary>
/// <param name="RemovedPatientCount">Number of patient records removed.</param>
/// <param name="RemovedPendingRequestCount">Number of pending requests removed.</param>
/// <param name="RemovedProposedChangeCount">Number of proposed changes removed.</param>
/// <param name="IsHealthcareProvider">Whether the user still has provider role after teardown.</param>
public sealed record HealthcareProviderTeardownResult(
    int RemovedPatientCount,
    int RemovedPendingRequestCount,
    int RemovedProposedChangeCount,
    bool IsHealthcareProvider);
