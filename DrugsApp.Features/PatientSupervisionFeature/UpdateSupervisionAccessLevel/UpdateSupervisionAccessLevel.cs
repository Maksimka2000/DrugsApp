using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;

public sealed record UpdateSupervisionAccessLevelRequest(string AccessLevelCode);

public sealed record UpdateSupervisionAccessLevelCommand(
    Guid PatientUserId,
    Guid ConnectionId,
    string AccessLevelCode) : IRequest<ErrorOr<UpdateSupervisionAccessLevelResponseDto>>;

internal sealed class UpdateSupervisionAccessLevelHandler(
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IUserProfileRepository userProfileRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<UpdateSupervisionAccessLevelCommand, ErrorOr<UpdateSupervisionAccessLevelResponseDto>>
{
    public async Task<ErrorOr<UpdateSupervisionAccessLevelResponseDto>> Handle(
        UpdateSupervisionAccessLevelCommand command,
        CancellationToken cancellationToken)
    {
        SupervisionConnection? connection = await supervisionConnectionRepository.GetByIdForPatientAsync(
            command.PatientUserId,
            command.ConnectionId,
            track: true,
            cancellationToken);

        if (connection is null)
        {
            return Error.NotFound(
                description: "Supervision connection not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.ConnectionId.ToString()
                });
        }

        if (!SupervisionAccessLevelCatalog.TryParse(command.AccessLevelCode, out SupervisionAccessLevel accessLevel))
        {
            return Error.Validation(description: "Invalid access level code.");
        }

        SupervisionAccessLevel previousAccessLevel = connection.AccessLevelCode;
        DateTime nowUtc = DateTime.UtcNow;
        ErrorOr<Success> updateResult = connection.UpdateAccessLevel(accessLevel, nowUtc);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        if (previousAccessLevel != accessLevel)
        {
            UserProfile? patientProfile = await userProfileRepository.GetByUserIdAsync(command.PatientUserId, cancellationToken);
            string patientSummary = patientProfile is null
                ? "A patient"
                : $"{patientProfile.FirstName} {patientProfile.LastName}".Trim();

            ErrorOr<Success> projectionResult = await notificationProjectionService.ProjectAsync(
                SupervisionNotificationMappings.BuildAccessLevelChangedDraft(
                    connection.HealthcareProfessionalUserId,
                    connection.Id,
                    patientSummary,
                    ToApiAccessLevelCode(connection.AccessLevelCode)),
                cancellationToken);
            if (projectionResult.IsError)
            {
                return projectionResult.Errors;
            }
        }

        return new UpdateSupervisionAccessLevelResponseDto(
            ConnectionId: connection.Id,
            AccessLevelCode: connection.AccessLevelCode.ToString(),
            UpdatedAtUtc: nowUtc);
    }

    private static string ToApiAccessLevelCode(SupervisionAccessLevel accessLevel) =>
        accessLevel switch
        {
            SupervisionAccessLevel.ViewAndModify => "ViewAndModify",
            SupervisionAccessLevel.ViewOnly => "ViewOnly",
            SupervisionAccessLevel.RestrictedView => "RestrictedView",
            SupervisionAccessLevel.BlockAccess => "BlockAccess",
            _ => accessLevel.ToString()
        };
}
