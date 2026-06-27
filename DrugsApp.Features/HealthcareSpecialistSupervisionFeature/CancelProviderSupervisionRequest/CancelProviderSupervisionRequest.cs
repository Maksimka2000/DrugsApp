using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderSupervisionRequest;

/// <summary>
/// Request payload for canceling a provider supervision request.
/// </summary>
/// <param name="RequestId">Identifier of the request to cancel.</param>
public sealed record CancelProviderSupervisionRequestRequest(Guid RequestId);

/// <summary>
/// Command to cancel a provider supervision request.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="RequestId">Identifier of the request to cancel.</param>
public sealed record CancelProviderSupervisionRequestCommand(Guid ProviderUserId, Guid RequestId)
    : IRequest<ErrorOr<CancelProviderSupervisionRequestResponseDto>>;

internal sealed class CancelProviderSupervisionRequestHandler(
    ISupervisionRequestRepository supervisionRequestRepository,
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<CancelProviderSupervisionRequestCommand, ErrorOr<CancelProviderSupervisionRequestResponseDto>>
{
    public async Task<ErrorOr<CancelProviderSupervisionRequestResponseDto>> Handle(
        CancelProviderSupervisionRequestCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> profileGuard = await HealthcareProviderProfileGuard.EnsureExistsAsync(
            healthcareProviderProfileRepository,
            command.ProviderUserId,
            cancellationToken);
        if (profileGuard.IsError)
        {
            return profileGuard.Errors;
        }

        SupervisionRequest? request = await supervisionRequestRepository.GetByIdForProviderAsync(
            command.ProviderUserId,
            command.RequestId,
            track: true,
            cancellationToken);

        if (request is null)
        {
            return Error.NotFound(
                description: "Supervision request not found.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionRequest",
                    ["resourceId"] = command.RequestId.ToString()
                });
        }

        DateTime nowUtc = DateTime.UtcNow;

        if (request.RequestState != SupervisionRequestState.Pending)
        {
            string finalState = request.RequestState.ToString();
            DateTime resolvedAtUtc = request.ResolvedAtUtc ?? nowUtc;

            supervisionRequestRepository.Delete(request);

            return new CancelProviderSupervisionRequestResponseDto(
                RequestId: request.Id,
                RequestState: finalState,
                ResolvedAtUtc: resolvedAtUtc);
        }

        ErrorOr<Success> cancelResult = request.Cancel(nowUtc, command.ProviderUserId);
        if (cancelResult.IsError)
        {
            return cancelResult.Errors;
        }

        ErrorOr<Success> syncResult = await notificationProjectionService.SyncActionStateAsync(
            request.PatientUserId,
            sourceEventKey: SupervisionNotificationMappings.BuildRequestSourceEventKey(request.Id),
            actionState: Domain.NotificationsFeature.Enums.NotificationActionState.Completed,
            requiresAction: false,
            cancellationToken);
        if (syncResult.IsError)
        {
            return syncResult.Errors;
        }

        string resolvedState = request.RequestState.ToString();
        supervisionRequestRepository.Delete(request);

        return new CancelProviderSupervisionRequestResponseDto(
            RequestId: request.Id,
            RequestState: resolvedState,
            ResolvedAtUtc: nowUtc);
    }
}
