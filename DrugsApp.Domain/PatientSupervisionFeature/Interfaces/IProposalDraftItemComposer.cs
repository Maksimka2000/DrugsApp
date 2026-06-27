using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Composes provider draft DTOs into validated proposal draft items with display snapshots.
/// </summary>
public interface IProposalDraftItemComposer
{
    /// <summary>
    /// Composes validated draft items with display snapshots for the specified patient.
    /// </summary>
    /// <param name="patientUserId">Patient owner for whom the draft is being composed.</param>
    /// <param name="draftItems">Raw provider-supplied draft items.</param>
    /// <param name="cancellationToken">Propagation token to cancel composition.</param>
    /// <returns>Collection of composed draft items or a domain error.</returns>
    Task<ErrorOr<IReadOnlyCollection<ComposedProposalDraftItem>>> ComposeAsync(
        Guid patientUserId,
        IReadOnlyCollection<ProposalDraftItemInput> draftItems,
        CancellationToken cancellationToken);
}

/// <summary>
/// Provider-facing draft item input before persistence.
/// </summary>
public sealed record ProposalDraftItemInput(
    string CategoryCode,
    string? Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Note,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Draft item enriched with patient-preview snapshots.
/// </summary>
public sealed record ComposedProposalDraftItem(
    ProposalDraftItem Draft,
    string DisplayName);
