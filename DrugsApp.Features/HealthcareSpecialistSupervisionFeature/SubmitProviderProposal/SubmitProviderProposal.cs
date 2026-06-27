using DrugsApp.Domain.PatientSupervisionFeature;
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

/// <summary>
/// Request payload for submitting a provider proposal.
/// </summary>
/// <param name="DraftItems">Proposal draft items.</param>
public sealed record SubmitProviderProposalRequest(
    IReadOnlyCollection<ProposalDraftItemDto> DraftItems);

/// <summary>
/// Command to submit a provider proposal for a patient.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="PatientId">Patient identifier.</param>
/// <param name="DraftItems">Draft items to submit.</param>
public sealed record SubmitProviderProposalCommand(
    Guid ProviderUserId,
    Guid PatientId,
    IReadOnlyCollection<ProposalDraftItemDto> DraftItems)
    : IRequest<ErrorOr<SubmitProviderProposalResponseDto>>;

internal sealed class SubmitProviderProposalHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IProposalDraftItemComposer proposalDraftItemComposer,
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

        SupervisionConnection? connection = await supervisionConnectionRepository.GetActiveForProviderAndPatientAsync(
            command.ProviderUserId,
            command.PatientId,
            track: false,
            cancellationToken);

        if (connection is null)
        {
            return Error.NotFound(
                description: "Active supervision connection not found for patient.",
                metadata: new()
                {
                    ["resourceType"] = "SupervisionConnection",
                    ["resourceId"] = command.PatientId.ToString()
                });
        }

        if (connection.AccessLevelCode != SupervisionAccessLevel.ViewAndModify)
        {
            return Error.Forbidden(description: "Provider lacks modify access level.");
        }

        IReadOnlyCollection<ProposalDraftItemInput> draftInputs = command.DraftItems
            .Select(item => item.MapToInput())
            .ToArray();

        ErrorOr<IReadOnlyCollection<ComposedProposalDraftItem>> composedResult =
            await proposalDraftItemComposer.ComposeAsync(
                connection.PatientUserId,
                draftInputs,
                cancellationToken);
        if (composedResult.IsError)
        {
            return composedResult.Errors;
        }

        IReadOnlyCollection<ProposalDraftItem> draftItems = composedResult.Value
            .Select(item => item.Draft)
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

        List<MedicalChangeProposalItem> items = new();
        foreach ((ComposedProposalDraftItem composed, int idx) entry in composedResult.Value.Select((item, idx) => (item, idx)))
        {
            ProposalDraftItem draft = entry.composed.Draft;
            var intakeTimes = (draft.IntakeTimes ?? Array.Empty<TimeOnly>())
                .Select(time => MedicalChangeProposalItemIntakeTime.Create(Guid.Empty, time))
                .Select(result => result.Match(
                    success => success,
                    errors => throw new InvalidOperationException("Invalid intake time payload.")))
                .ToList();

            ErrorOr<MedicalChangeProposalItem> itemResult = MedicalChangeProposalItem.Create(
                proposalId: Guid.Empty,
                categoryCode: draft.CategoryCode,
                source: draft.Source,
                relatedCatalogId: draft.RelatedCatalogId,
                customValue: draft.CustomValue,
                displayNameSnapshot: entry.composed.DisplayName,
                secondaryTextSnapshot: null,
                startDate: draft.StartDate,
                endDate: draft.EndDate,
                note: draft.Note,
                applyPayload: draft.ApplyPayload,
                sortOrder: entry.idx,
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
            proposalSummary: BuildProposalSummary(items),
            submittedAtUtc: nowUtc,
            items: items);

        if (proposalResult.IsError)
        {
            return proposalResult.Errors;
        }

        MedicalChangeProposal proposal = proposalResult.Value;

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

    private static string BuildProposalSummary(IReadOnlyCollection<MedicalChangeProposalItem> items)
    {
        if (items.Count == 1)
        {
            return $"Proposed addition: {items.First().DisplayNameSnapshot}";
        }

        return $"Proposed {items.Count} medical additions";
    }
}
