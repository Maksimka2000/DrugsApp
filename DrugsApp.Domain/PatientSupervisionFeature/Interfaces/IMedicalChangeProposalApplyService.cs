using System.Threading.Tasks;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Applies approved proposal items into patient-owned medical domains.
/// </summary>
public interface IMedicalChangeProposalApplyService
{
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
