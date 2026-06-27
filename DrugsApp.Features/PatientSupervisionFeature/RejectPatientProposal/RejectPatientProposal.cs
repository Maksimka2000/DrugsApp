using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectPatientProposal;

/// <summary>
/// Request payload to reject a patient proposal.
/// </summary>
/// <param name="ProposalId">Proposal identifier.</param>
public sealed record RejectPatientProposalRequest(Guid ProposalId);

/// <summary>
/// Command to reject a patient proposal.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="ProposalId">Proposal identifier.</param>
public sealed record RejectPatientProposalCommand(Guid PatientUserId, Guid ProposalId)
    : IRequest<ErrorOr<RejectPatientProposalResponseDto>>;

internal sealed class RejectPatientProposalHandler(
    IMedicalChangeProposalRepository proposalRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<RejectPatientProposalCommand, ErrorOr<RejectPatientProposalResponseDto>>
{
    public async Task<ErrorOr<RejectPatientProposalResponseDto>> Handle(
        RejectPatientProposalCommand command,
        CancellationToken cancellationToken)
    {
        MedicalChangeProposal? proposal = await proposalRepository.GetTrackedByIdForPatientAsync(
            command.PatientUserId,
            command.ProposalId,
            cancellationToken);

        if (proposal is null)
        {
            return Error.NotFound(
                description: "Proposal not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "MedicalChangeProposal",
                    ["resourceId"] = command.ProposalId.ToString()
                });
        }

        if (proposal.ProposalState != MedicalChangeProposalState.Pending)
        {
            return Error.Conflict(description: "Proposal is not pending.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        ErrorOr<Success> rejectResult = proposal.Reject(nowUtc, command.PatientUserId);
        if (rejectResult.IsError)
        {
            return rejectResult.Errors;
        }

        ErrorOr<Success> syncResult = await notificationProjectionService.SyncActionStateAsync(
            command.PatientUserId,
            sourceEventKey: SupervisionNotificationMappings.BuildProposalSourceEventKey(proposal.Id),
            actionState: Domain.NotificationsFeature.Enums.NotificationActionState.Rejected,
            requiresAction: false,
            cancellationToken);
        if (syncResult.IsError)
        {
            return syncResult.Errors;
        }

        return new RejectPatientProposalResponseDto(
            ProposalId: proposal.Id,
            ProposalState: proposal.ProposalState.ToString(),
            ResolvedAtUtc: nowUtc);
    }
}
