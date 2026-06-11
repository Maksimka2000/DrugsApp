using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderRequest;

public sealed record CancelProviderRequestRequest(Guid RequestId);

public sealed record CancelProviderRequestCommand(Guid ProviderUserId, Guid RequestId)
    : IRequest<ErrorOr<CancelProviderRequestResponseDto>>;

internal sealed class CancelProviderRequestHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionRequestRepository supervisionRequestRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<CancelProviderRequestCommand, ErrorOr<CancelProviderRequestResponseDto>>
{
    public async Task<ErrorOr<CancelProviderRequestResponseDto>> Handle(
        CancelProviderRequestCommand command,
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

        return new CancelProviderRequestResponseDto(
            RequestId: request.Id,
            RequestState: request.RequestState.ToString(),
            ResolvedAtUtc: nowUtc);
    }
}
