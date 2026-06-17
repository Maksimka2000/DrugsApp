using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.RemoveHealthcareProviderProfile;

/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
public sealed record RemoveHealthcareProviderProfileCommand(Guid UserId) : IRequest<ErrorOr<RemoveHealthcareProviderProfileResponse>>;

/// <summary>
/// 
/// </summary>
/// <param name="RemovedPatientCount"></param>
/// <param name="RemovedPendingRequestCount"></param>
/// <param name="RemovedProposedChangeCount"></param>
/// <param name="IsHealthcareProvider"></param>
public sealed record RemoveHealthcareProviderProfileResponse(
    int RemovedPatientCount,
    int RemovedPendingRequestCount,
    int RemovedProposedChangeCount,
    bool IsHealthcareProvider);

internal sealed class RemoveHealthcareProviderProfileHandler(
    IHealthcareProviderTeardownService teardownService)
    : IRequestHandler<RemoveHealthcareProviderProfileCommand, ErrorOr<RemoveHealthcareProviderProfileResponse>>
{
    public async Task<ErrorOr<RemoveHealthcareProviderProfileResponse>> Handle(RemoveHealthcareProviderProfileCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<HealthcareProviderTeardownResult> result = await teardownService.RemoveProviderRoleAsync(command.UserId, cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        return new RemoveHealthcareProviderProfileResponse(
            result.Value.RemovedPatientCount,
            result.Value.RemovedPendingRequestCount,
            result.Value.RemovedProposedChangeCount,
            result.Value.IsHealthcareProvider);
    }
}
