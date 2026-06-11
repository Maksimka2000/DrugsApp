using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

public sealed record SubmitProviderProposalRequest(
    IReadOnlyCollection<ProposalDraftItemDto> DraftItems);

public sealed record SubmitProviderProposalCommand(
    Guid ProviderUserId,
    Guid ConnectionId,
    IReadOnlyCollection<ProposalDraftItemDto> DraftItems)
    : IRequest<ErrorOr<SubmitProviderProposalResponseDto>>;

internal sealed class SubmitProviderProposalHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IMedicalChangeProposalDraftValidationService draftValidationService,
    IMedicalChangeProposalRepository proposalRepository,
    INotificationProjectionService notificationProjectionService)
    : IRequestHandler<SubmitProviderProposalCommand, ErrorOr<SubmitProviderProposalResponseDto>>
{
    public async Task<ErrorOr<SubmitProviderProposalResponseDto>> Handle(
        SubmitProviderProposalCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> profileGuard = await HealthcareProviderProfileGuard.EnsureExistsAsync(
            healthcareProviderProfileRepository,
            command.ProviderUserId,
            cancellationToken);
        if (profileGuard.IsError)
        {
            return profileGuard.Errors;
        }

        SupervisionConnection? connection = await supervisionConnectionRepository.GetByIdForProviderAsync(
            command.ProviderUserId,
            command.ConnectionId,
            track: false,
            cancellationToken);

        if (connection is null || connection.ConnectionState != SupervisionConnectionState.Active)
        {
            return Error.NotFound(
                description: "Active supervision connection not found.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.ConnectionId.ToString()
                });
        }

        if (connection.AccessLevelCode != SupervisionAccessLevel.ViewAndModify)
        {
            return Error.Forbidden(description: "Provider lacks modify access level.");
        }

        IReadOnlyCollection<ProposalDraftItem> draftItems = command.DraftItems
            .Select((item, index) => new ProposalDraftItem(
                Enum.Parse<MedicalChangeProposalCategory>(item.CategoryCode, ignoreCase: true),
                Enum.Parse<MedicalChangeProposalSource>(item.Source, ignoreCase: true),
                item.RelatedCatalogId,
                item.CustomValue,
                item.ApplyPayload,
                item.IntakeTimes))
            .ToArray();

        ErrorOr<Success> validationResult = await draftValidationService.ValidateAsync(
            connection.PatientUserId,
            draftItems,
            cancellationToken);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        DateTime nowUtc = DateTime.UtcNow;

        // Build items
        List<MedicalChangeProposalItem> items = new();
        foreach ((ProposalDraftItem item, int idx) draft in draftItems.Select((item, idx) => (item, idx)))
        {
            // Placeholder mapping until real display names and details are wired
            string displayName = draft.item.CustomValue ?? draft.item.CategoryCode.ToString();
            string? secondaryText = null;
            var intakeTimes = (draft.item.IntakeTimes ?? Array.Empty<TimeOnly>())
                .Select(time => MedicalChangeProposalItemIntakeTime.Create(Guid.Empty, time))
                .Select(result => result.Match(
                    success => success,
                    errors => throw new InvalidOperationException("Invalid intake time payload.")))
                .ToList();

            ErrorOr<MedicalChangeProposalItem> itemResult = MedicalChangeProposalItem.Create(
                proposalId: Guid.Empty,
                categoryCode: draft.item.CategoryCode,
                source: draft.item.Source,
                relatedCatalogId: draft.item.RelatedCatalogId,
                customValue: draft.item.CustomValue,
                displayNameSnapshot: displayName,
                secondaryTextSnapshot: secondaryText,
                startDate: null,
                endDate: null,
                note: null,
                applyPayload: draft.item.ApplyPayload,
                sortOrder: draft.idx,
                intakeTimes: intakeTimes);

            if (itemResult.IsError)
            {
                return itemResult.Errors;
            }

            items.Add(itemResult.Value);
        }

        ErrorOr<MedicalChangeProposal> proposalResult = MedicalChangeProposal.Create(
            connectionId: connection.Id,
            patientUserId: connection.PatientUserId,
            healthcareProfessionalUserId: connection.HealthcareProfessionalUserId,
            accessLevelSnapshot: connection.AccessLevelCode,
            proposalSummary: $"Proposal submitted on {nowUtc:O}",
            submittedAtUtc: nowUtc,
            items: items);

        if (proposalResult.IsError)
        {
            return proposalResult.Errors;
        }

        MedicalChangeProposal proposal = proposalResult.Value;

        // Back-fill generated IDs into children before persistence.
        foreach (MedicalChangeProposalItem item in items)
        {
            item.BindToProposal(proposal.Id);
            foreach (MedicalChangeProposalItemIntakeTime intake in item.IntakeTimes)
            {
                intake.BindToProposalItem(item.Id);
            }
        }

        await proposalRepository.AddAsync(proposal, cancellationToken);

        ErrorOr<Success> projectionResult = await notificationProjectionService.ProjectAsync(
            SupervisionNotificationMappings.BuildProposalSubmittedDraft(
                connection.PatientUserId,
                proposal.Id,
                proposal.ProposalSummary),
            cancellationToken);
        if (projectionResult.IsError)
        {
            return projectionResult.Errors;
        }

        return new SubmitProviderProposalResponseDto(
            ProposalId: proposal.Id,
            ProposalState: proposal.ProposalState.ToString(),
            SubmittedAtUtc: proposal.SubmittedAtUtc,
            ProposalSummary: proposal.ProposalSummary);
    }
}
