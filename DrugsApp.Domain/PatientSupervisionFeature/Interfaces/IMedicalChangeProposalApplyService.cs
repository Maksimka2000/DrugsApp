using System.Threading.Tasks;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Applies approved proposal items into patient-owned medical domains.
/// </summary>
public interface IMedicalChangeProposalApplyService
{
    /// <summary>
    /// Applies a confirmed proposal and its items to patient-owned medical records.
    /// </summary>
    /// <param name="patientUserId">Patient owner of the proposal.</param>
    /// <param name="proposal">Aggregate proposal to apply.</param>
    /// <param name="items">Immutable list of proposal items to process.</param>
    /// <param name="cancellationToken">Propagation token to cancel the apply operation.</param>
    /// <returns>Result describing created or invalidated records.</returns>
    Task<ErrorOr<ApplyProposalResult>> ApplyAsync(
        Guid patientUserId,
        MedicalChangeProposal proposal,
        IReadOnlyCollection<MedicalChangeProposalItem> items,
        CancellationToken cancellationToken);
}

/// <summary>
/// Aggregate result describing applied regular-intake records.
/// </summary>
public sealed record ApplyProposalResult(
    int RegularDrugCount,
    int RegularSupplementCount,
    int RegularFoodCount,
    string? InvalidatedReason);
