using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectSupervisionRequest;

public sealed record RejectSupervisionRequestRequest(Guid RequestId);

public sealed record RejectSupervisionRequestCommand(Guid PatientUserId, Guid RequestId)
    : IRequest<ErrorOr<RejectSupervisionRequestResponseDto>>;

internal sealed class RejectSupervisionRequestHandler(
    ISupervisionRequestRepository supervisionRequestRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<RejectSupervisionRequestCommand, ErrorOr<RejectSupervisionRequestResponseDto>>
{
    public async Task<ErrorOr<RejectSupervisionRequestResponseDto>> Handle(
        RejectSupervisionRequestCommand command,
        CancellationToken cancellationToken)
    {
        SupervisionRequest? request = await supervisionRequestRepository.GetByIdForPatientAsync(
            command.PatientUserId,
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
        ErrorOr<Success> result = request.Reject(nowUtc, command.PatientUserId);
        if (result.IsError)
        {
            return result.Errors;
        }

        ErrorOr<Success> syncResult = await notificationProjectionService.SyncActionStateAsync(
            command.PatientUserId,
            sourceEventKey: SupervisionNotificationMappings.BuildRequestSourceEventKey(request.Id),
            actionState: Domain.NotificationsFeature.Enums.NotificationActionState.Rejected,
            requiresAction: false,
            cancellationToken);
        if (syncResult.IsError)
        {
            return syncResult.Errors;
        }

        return new RejectSupervisionRequestResponseDto(
            RequestId: request.Id,
            RequestState: request.RequestState.ToString(),
            ResolvedAtUtc: nowUtc);
    }
}
