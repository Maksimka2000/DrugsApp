using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.ApproveSupervisionRequest;

public sealed record ApproveSupervisionRequestRequest(string AccessLevelCode);

public sealed record ApproveSupervisionRequestCommand(
    Guid PatientUserId,
    Guid RequestId,
    string AccessLevelCode) : IRequest<ErrorOr<ApproveSupervisionRequestResponseDto>>;

internal sealed class ApproveSupervisionRequestHandler(
    ISupervisionRequestRepository supervisionRequestRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<ApproveSupervisionRequestCommand, ErrorOr<ApproveSupervisionRequestResponseDto>>
{
    public async Task<ErrorOr<ApproveSupervisionRequestResponseDto>> Handle(
        ApproveSupervisionRequestCommand command,
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
        ErrorOr<Success> approveResult = request.Approve(nowUtc, command.PatientUserId);
        if (approveResult.IsError)
        {
            return approveResult.Errors;
        }

        if (!SupervisionAccessLevelCatalog.TryParse(command.AccessLevelCode, out SupervisionAccessLevel accessLevel))
        {
            return Error.Validation(description: "Invalid access level code.");
        }

        SupervisionConnection? existingConnection = await supervisionConnectionRepository.GetActiveOrRemovedForPairAsync(
            request.PatientUserId,
            request.HealthcareProfessionalUserId,
            cancellationToken);

        ErrorOr<SupervisionConnection> connectionResult = SupervisionConnection.CreateOrReactivate(
            request.PatientUserId,
            request.HealthcareProfessionalUserId,
            accessLevel,
            nowUtc);

        if (connectionResult.IsError)
        {
            return connectionResult.Errors;
        }

        SupervisionConnection connection;
        if (existingConnection is null)
        {
            connection = connectionResult.Value;
            await supervisionConnectionRepository.AddAsync(connection, cancellationToken);
        }
        else
        {
            connection = existingConnection;
            ErrorOr<Success> connectionUpdateResult = connection.ConnectionState == SupervisionConnectionState.Removed
                ? connection.Reactivate(accessLevel, nowUtc)
                : connection.UpdateAccessLevel(accessLevel, nowUtc);
            if (connectionUpdateResult.IsError)
            {
                return connectionUpdateResult.Errors;
            }
        }

        ErrorOr<Success> syncResult = await notificationProjectionService.SyncActionStateAsync(
            command.PatientUserId,
            sourceEventKey: SupervisionNotificationMappings.BuildRequestSourceEventKey(request.Id),
            actionState: Domain.NotificationsFeature.Enums.NotificationActionState.Approved,
            requiresAction: false,
            cancellationToken);
        if (syncResult.IsError)
        {
            return syncResult.Errors;
        }

        Guid requestId = request.Id;
        string requestState = request.RequestState.ToString();
        supervisionRequestRepository.Delete(request);

        return new ApproveSupervisionRequestResponseDto(
            RequestId: requestId,
            RequestState: requestState,
            ConnectionId: connection.Id,
            AccessLevelCode: connection.AccessLevelCode.ToString(),
            ApprovedAtUtc: nowUtc);
    }
}
