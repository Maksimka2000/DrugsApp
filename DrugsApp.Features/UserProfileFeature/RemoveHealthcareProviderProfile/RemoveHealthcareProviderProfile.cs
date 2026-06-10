using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.RemoveHealthcareProviderProfile;

public sealed record RemoveHealthcareProviderProfileCommand(Guid UserId) : IRequest<ErrorOr<RemoveHealthcareProviderProfileResponse>>;

public sealed record RemoveHealthcareProviderProfileResponse(
    int RemovedPatientCount,
    int RemovedPendingRequestCount,
    int RemovedProposedChangeCount,
    bool IsHealthcareProvider);

internal sealed class RemoveHealthcareProviderProfileHandler(
    IHealthcareProviderTeardownService _teardownService)
    : IRequestHandler<RemoveHealthcareProviderProfileCommand, ErrorOr<RemoveHealthcareProviderProfileResponse>>
{
    public async Task<ErrorOr<RemoveHealthcareProviderProfileResponse>> Handle(RemoveHealthcareProviderProfileCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<HealthcareProviderTeardownResult> result = await _teardownService.RemoveProviderRoleAsync(command.UserId, cancellationToken);
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
