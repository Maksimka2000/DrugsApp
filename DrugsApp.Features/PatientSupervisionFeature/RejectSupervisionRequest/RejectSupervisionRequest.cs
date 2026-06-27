using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectSupervisionRequest;

/// <summary>
/// Request payload to reject a supervision request.
/// </summary>
/// <param name="RequestId">Identifier of the request to reject.</param>
public sealed record RejectSupervisionRequestRequest(Guid RequestId);

/// <summary>
/// Command to reject a supervision request for a patient.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="RequestId">Request identifier.</param>
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

        Guid requestId = request.Id;
        string requestState = request.RequestState.ToString();
        supervisionRequestRepository.Delete(request);

        return new RejectSupervisionRequestResponseDto(
            RequestId: requestId,
            RequestState: requestState,
            ResolvedAtUtc: nowUtc);
    }
}
