using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

public sealed record RemoveSupervisionConnectionCommand(
    Guid CallerUserId,
    Guid? ExpectedHealthcareProfessionalUserId = null,
    Guid? ExpectedPatientUserId = null) : IRequest<ErrorOr<RemoveSupervisionConnectionResponseDto>>;

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
        ErrorOr<(Guid PatientUserId, Guid ProviderUserId)> pairResult = ResolvePair(command);
        if (pairResult.IsError)
        {
            return pairResult.Errors;
        }

        (Guid patientUserId, Guid providerUserId) = pairResult.Value;

        SupervisionConnection? connection = await supervisionConnectionRepository.GetActiveForProviderAndPatientAsync(
            providerUserId,
            patientUserId,
            track: true,
            cancellationToken);

        if (connection is null)
        {
            return Error.NotFound(
                description: "Active supervision connection not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["patientUserId"] = patientUserId.ToString(),
                    ["healthcareProfessionalUserId"] = providerUserId.ToString()
                });
        }

        bool removedByPatient = connection.PatientUserId == command.CallerUserId;
        bool removedByProvider = connection.HealthcareProfessionalUserId == command.CallerUserId;
        if (!removedByPatient && !removedByProvider)
        {
            return Error.NotFound(
                description: "Supervision connection not found for caller.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["patientUserId"] = patientUserId.ToString(),
                    ["healthcareProfessionalUserId"] = providerUserId.ToString()
                });
        }

        if (connection.ConnectionState != SupervisionConnectionState.Active)
        {
            return Error.Conflict(description: "Connection already removed.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        Guid connectionId = connection.Id;

        ErrorOr<Success> notificationResult = removedByPatient
            ? await NotifyProviderPatientRemovedAsync(connection, cancellationToken)
            : await NotifyPatientProviderRemovedAsync(connection, cancellationToken);
        if (notificationResult.IsError)
        {
            return notificationResult.Errors;
        }

        int deletedProposalCount = await medicalChangeProposalRepository.DeleteByConnectionAsync(
            connectionId,
            cancellationToken);

        supervisionConnectionRepository.Delete(connection);

        return new RemoveSupervisionConnectionResponseDto(
            ConnectionId: connectionId,
            ConnectionState: SupervisionConnectionState.Removed.ToString(),
            RemovedAtUtc: nowUtc,
            InvalidatedProposalCount: deletedProposalCount == 0 ? null : deletedProposalCount);
    }

    private static ErrorOr<(Guid PatientUserId, Guid ProviderUserId)> ResolvePair(
        RemoveSupervisionConnectionCommand command)
    {
        if (command.ExpectedHealthcareProfessionalUserId is Guid providerUserId)
        {
            return (command.CallerUserId, providerUserId);
        }

        if (command.ExpectedPatientUserId is Guid patientUserId)
        {
            return (patientUserId, command.CallerUserId);
        }

        return Error.Validation(description: "Counterparty user id is required.");
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
