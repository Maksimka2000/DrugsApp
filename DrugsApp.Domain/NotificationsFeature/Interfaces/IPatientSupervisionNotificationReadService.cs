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
    /// <summary>
    /// Loads supervision request details for a notification targeting the specified user.
    /// </summary>
    /// <param name="requestId">Identifier of the supervision request to hydrate.</param>
    /// <param name="userId">Owner user identifier for whom the notification is prepared.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Hydrated request detail payload or a domain error.</returns>
    Task<ErrorOr<SupervisionRequestNotificationDetail>> GetRequestDetailAsync(
        Guid requestId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads medical change proposal details for a notification targeting the specified user.
    /// </summary>
    /// <param name="proposalId">Identifier of the proposal to hydrate.</param>
    /// <param name="userId">Owner user identifier for whom the notification is prepared.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Hydrated proposal detail payload or a domain error.</returns>
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
