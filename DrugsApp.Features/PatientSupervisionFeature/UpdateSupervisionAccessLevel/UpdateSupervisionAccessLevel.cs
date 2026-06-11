using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;

public sealed record UpdateSupervisionAccessLevelRequest(Guid ConnectionId, string AccessLevelCode);

public sealed record UpdateSupervisionAccessLevelCommand(
    Guid PatientUserId,
    Guid ConnectionId,
    string AccessLevelCode) : IRequest<ErrorOr<UpdateSupervisionAccessLevelResponseDto>>;

internal sealed class UpdateSupervisionAccessLevelHandler(
    ISupervisionConnectionRepository supervisionConnectionRepository)
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

        if (!Enum.TryParse<SupervisionAccessLevel>(command.AccessLevelCode, ignoreCase: true, out SupervisionAccessLevel accessLevel))
        {
            return Error.Validation(description: "Invalid access level code.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        ErrorOr<Success> updateResult = connection.UpdateAccessLevel(accessLevel, nowUtc);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return new UpdateSupervisionAccessLevelResponseDto(
            ConnectionId: connection.Id,
            AccessLevelCode: connection.AccessLevelCode.ToString(),
            UpdatedAtUtc: nowUtc);
    }
}
