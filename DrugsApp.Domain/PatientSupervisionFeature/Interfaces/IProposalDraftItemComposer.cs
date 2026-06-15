using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Interfaces;

/// <summary>
/// Composes provider draft DTOs into validated proposal draft items with display snapshots.
/// </summary>
public interface IProposalDraftItemComposer
{
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
