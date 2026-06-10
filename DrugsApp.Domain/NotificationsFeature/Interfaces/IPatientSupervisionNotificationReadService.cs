using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.NotificationsFeature.Interfaces;

/// <summary>
/// Read facade aligned with PatientSupervisionFeature detail contracts for notification hydration.
/// </summary>
public interface IPatientSupervisionNotificationReadService
{
    Task<ErrorOr<SupervisionRequestNotificationDetail>> GetRequestDetailAsync(
        Guid requestId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<ErrorOr<MedicalChangeProposalNotificationDetail>> GetProposalDetailAsync(
        Guid proposalId,
        Guid userId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Supervision request detail block for notification hydration.
/// </summary>
public sealed record SupervisionRequestNotificationDetail(
    Guid RequestId,
    string HealthcareProfessionalSummary,
    string? RequestMessage,
    IReadOnlyList<string> AvailableAccessLevels,
    string? SelectedAccessLevelCode,
    string CurrentRequestState);

/// <summary>
/// Medical change proposal detail block for notification hydration.
/// </summary>
public sealed record MedicalChangeProposalNotificationDetail(
    Guid ProposalId,
    string HealthcareProfessionalSummary,
    string ProposalSummary,
    string? InteractionReportSummary,
    IReadOnlyList<MedicalChangeProposalSectionDetail> Sections,
    string? ProposalNote,
    string CurrentProposalState);

/// <summary>
/// Proposal section snapshot for notification hydration.
/// </summary>
public sealed record MedicalChangeProposalSectionDetail(
    string SectionCode,
    string SectionTitle,
    string SummaryText);
