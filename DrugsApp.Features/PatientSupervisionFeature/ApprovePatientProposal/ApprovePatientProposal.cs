using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.ApprovePatientProposal;

public sealed record ApprovePatientProposalRequest(Guid ProposalId);

public sealed record ApprovePatientProposalCommand(Guid PatientUserId, Guid ProposalId)
    : IRequest<ErrorOr<ApprovePatientProposalResponseDto>>;

internal sealed class ApprovePatientProposalHandler(
    IMedicalChangeProposalRepository proposalRepository,
    ISupervisionConnectionRepository connectionRepository,
    IMedicalChangeProposalApplyService applyService,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<ApprovePatientProposalCommand, ErrorOr<ApprovePatientProposalResponseDto>>
{
    public async Task<ErrorOr<ApprovePatientProposalResponseDto>> Handle(
        ApprovePatientProposalCommand command,
        CancellationToken cancellationToken)
    {
        MedicalChangeProposal? proposal = await proposalRepository.GetTrackedDetailForPatientAsync(
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

        SupervisionConnection? connection = await connectionRepository.GetByIdForPatientAsync(
            command.PatientUserId,
            proposal.SupervisionConnectionId,
            track: false,
            cancellationToken);

        if (connection is null || connection.ConnectionState != SupervisionConnectionState.Active)
        {
            proposal.Invalidate(DateTime.UtcNow);
            return Error.Conflict(description: "Connection not active; proposal invalidated.");
        }

        DateTime nowUtc = DateTime.UtcNow;

        ErrorOr<ApplyProposalResult> applyResult = await applyService.ApplyAsync(
            command.PatientUserId,
            proposal,
            proposal.Items.ToList(),
            cancellationToken);

        if (applyResult.IsError)
        {
            proposal.Invalidate(nowUtc);
            return applyResult.Errors;
        }

        ErrorOr<Success> approveResult = proposal.Approve(nowUtc, command.PatientUserId);
        if (approveResult.IsError)
        {
            return approveResult.Errors;
        }

        ErrorOr<Success> syncResult = await notificationProjectionService.SyncActionStateAsync(
            command.PatientUserId,
            sourceEventKey: SupervisionNotificationMappings.BuildProposalSourceEventKey(proposal.Id),
            actionState: Domain.NotificationsFeature.Enums.NotificationActionState.Approved,
            requiresAction: false,
            cancellationToken);
        if (syncResult.IsError)
        {
            return syncResult.Errors;
        }

        return new ApprovePatientProposalResponseDto(
            ProposalId: proposal.Id,
            ProposalState: proposal.ProposalState.ToString(),
            ResolvedAtUtc: nowUtc,
            CreatedRecordCounts: new ProposalCreatedRecordCountsDto(
                RegularDrug: applyResult.Value.RegularDrugCount,
                RegularSupplement: applyResult.Value.RegularSupplementCount,
                RegularFood: applyResult.Value.RegularFoodCount),
            InvalidatedReason: applyResult.Value.InvalidatedReason);
    }
}
