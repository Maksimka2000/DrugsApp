using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

public sealed record RemoveSupervisionConnectionRequest(Guid ConnectionId);

public sealed record RemoveSupervisionConnectionCommand(
    Guid CallerUserId,
    Guid ConnectionId,
    bool CallerIsProvider) : IRequest<ErrorOr<RemoveSupervisionConnectionResponseDto>>;

internal sealed class RemoveSupervisionConnectionHandler(
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IMedicalChangeProposalRepository medicalChangeProposalRepository)
    : IRequestHandler<RemoveSupervisionConnectionCommand, ErrorOr<RemoveSupervisionConnectionResponseDto>>
{
    public async Task<ErrorOr<RemoveSupervisionConnectionResponseDto>> Handle(
        RemoveSupervisionConnectionCommand command,
        CancellationToken cancellationToken)
    {
        SupervisionConnection? connection = command.CallerIsProvider
            ? await supervisionConnectionRepository.GetByIdForProviderAsync(command.CallerUserId, command.ConnectionId, track: true, cancellationToken)
            : await supervisionConnectionRepository.GetByIdForPatientAsync(command.CallerUserId, command.ConnectionId, track: true, cancellationToken);

        if (connection is null)
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

        return new RemoveSupervisionConnectionResponseDto(
            ConnectionId: connection.Id,
            ConnectionState: connection.ConnectionState.ToString(),
            RemovedAtUtc: nowUtc,
            InvalidatedProposalCount: invalidatedCount == 0 ? null : invalidatedCount);
    }
}
