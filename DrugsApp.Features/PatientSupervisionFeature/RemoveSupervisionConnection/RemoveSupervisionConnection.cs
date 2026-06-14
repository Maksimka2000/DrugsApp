using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

public sealed record RemoveSupervisionConnectionRequest(Guid ConnectionId);

public sealed record RemoveSupervisionConnectionCommand(
    Guid CallerUserId,
    Guid ConnectionId) : IRequest<ErrorOr<RemoveSupervisionConnectionResponseDto>>;

internal sealed class RemoveSupervisionConnectionHandler(
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IMedicalChangeProposalRepository medicalChangeProposalRepository,
    IUserProfileRepository userProfileRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<RemoveSupervisionConnectionCommand, ErrorOr<RemoveSupervisionConnectionResponseDto>>
{
    public async Task<ErrorOr<RemoveSupervisionConnectionResponseDto>> Handle(
        RemoveSupervisionConnectionCommand command,
        CancellationToken cancellationToken)
    {
        SupervisionConnection? connection = await supervisionConnectionRepository.GetByIdAsync(
            command.ConnectionId,
            track: true,
            cancellationToken);

        if (connection is null)
        {
            return Error.NotFound(
                description: "Supervision connection not found.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.ConnectionId.ToString()
                });
        }

        bool removedByPatient = connection.PatientUserId == command.CallerUserId;
        bool removedByProvider = connection.HealthcareProfessionalUserId == command.CallerUserId;
        if (!removedByPatient && !removedByProvider)
        {
            return Error.NotFound(
                description: "Supervision connection not found for caller.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.ConnectionId.ToString()
                });
        }

        DateTime nowUtc = DateTime.UtcNow;
        ErrorOr<Success> removeResult = connection.Remove(nowUtc);
        if (removeResult.IsError)
        {
            return removeResult.Errors;
        }

        int invalidatedCount = await medicalChangeProposalRepository.InvalidatePendingByConnectionAsync(
            connection.Id,
            nowUtc,
            cancellationToken);

        ErrorOr<Success> notificationResult = removedByPatient
            ? await NotifyProviderPatientRemovedAsync(connection, cancellationToken)
            : await NotifyPatientProviderRemovedAsync(connection, cancellationToken);
        if (notificationResult.IsError)
        {
            return notificationResult.Errors;
        }

        return new RemoveSupervisionConnectionResponseDto(
            ConnectionId: connection.Id,
            ConnectionState: connection.ConnectionState.ToString(),
            RemovedAtUtc: nowUtc,
            InvalidatedProposalCount: invalidatedCount == 0 ? null : invalidatedCount);
    }

    private async Task<ErrorOr<Success>> NotifyProviderPatientRemovedAsync(
        SupervisionConnection connection,
        CancellationToken cancellationToken)
    {
        UserProfile? patientProfile = await userProfileRepository.GetByUserIdAsync(
            connection.PatientUserId,
            cancellationToken);
        string patientSummary = patientProfile is null
            ? "A patient"
            : $"{patientProfile.FirstName} {patientProfile.LastName}".Trim();

        return await notificationProjectionService.ProjectAsync(
            SupervisionNotificationMappings.BuildRemovedByPatientDraft(
                connection.HealthcareProfessionalUserId,
                connection.Id,
                connection.PatientUserId,
                patientSummary),
            cancellationToken);
    }

    private async Task<ErrorOr<Success>> NotifyPatientProviderRemovedAsync(
        SupervisionConnection connection,
        CancellationToken cancellationToken)
    {
        UserProfile? providerProfile = await userProfileRepository.GetByUserIdAsync(
            connection.HealthcareProfessionalUserId,
            cancellationToken);
        string providerSummary = providerProfile is null
            ? "A healthcare provider"
            : $"{providerProfile.FirstName} {providerProfile.LastName}".Trim();

        return await notificationProjectionService.ProjectAsync(
            SupervisionNotificationMappings.BuildRemovedByDoctorDraft(
                connection.PatientUserId,
                connection.Id,
                connection.HealthcareProfessionalUserId,
                providerSummary),
            cancellationToken);
    }
}
