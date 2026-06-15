using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;

/// <summary>
/// Lightweight proposal draft item used for validation/report generation.
/// </summary>
public sealed record ProposalDraftItem(
    MedicalChangeProposalCategory CategoryCode,
    MedicalChangeProposalSource Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    string ApplyPayload,
    IReadOnlyCollection<TimeOnly>? IntakeTimes,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? Note = null);
