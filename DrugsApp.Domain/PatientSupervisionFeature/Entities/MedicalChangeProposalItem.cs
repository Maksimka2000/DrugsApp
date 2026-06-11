using DrugsApp.Domain.Common;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Immutable add-only proposal item snapshot.
/// </summary>
public sealed class MedicalChangeProposalItem : BaseEntity
{
    // Attributes
    public Guid ProposalId { get; private set; }
    public MedicalChangeProposalCategory CategoryCode { get; private set; }
    public MedicalChangeProposalSource Source { get; private set; }
    public Guid? RelatedCatalogId { get; private set; }
    public string? CustomValue { get; private set; }
    public string DisplayNameSnapshot { get; private set; } = null!;
    public string? SecondaryTextSnapshot { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Note { get; private set; }
    public string ApplyPayload { get; private set; } = null!;
    public int SortOrder { get; private set; }

    private readonly List<MedicalChangeProposalItemIntakeTime> _intakeTimes = new();
    public IReadOnlyCollection<MedicalChangeProposalItemIntakeTime> IntakeTimes => _intakeTimes.AsReadOnly();

    // Factory
    public static ErrorOr<MedicalChangeProposalItem> Create(
        Guid proposalId,
        MedicalChangeProposalCategory categoryCode,
        MedicalChangeProposalSource source,
        Guid? relatedCatalogId,
        string? customValue,
        string displayNameSnapshot,
        string? secondaryTextSnapshot,
        DateOnly? startDate,
        DateOnly? endDate,
        string? note,
        string applyPayload,
        int sortOrder,
        IReadOnlyCollection<MedicalChangeProposalItemIntakeTime> intakeTimes)
    {
        if (sortOrder < 0)
        {
            return Error.Validation(description: "Sort order must be non-negative.");
        }

        if (string.IsNullOrWhiteSpace(displayNameSnapshot))
        {
            return Error.Validation(description: "Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(applyPayload))
        {
            return Error.Validation(description: "Apply payload is required.");
        }

        var item = new MedicalChangeProposalItem
        {
            ProposalId = proposalId,
            CategoryCode = categoryCode,
            Source = source,
            RelatedCatalogId = relatedCatalogId,
            CustomValue = NormalizeOptional(customValue),
            DisplayNameSnapshot = displayNameSnapshot.Trim(),
            SecondaryTextSnapshot = NormalizeOptional(secondaryTextSnapshot),
            StartDate = startDate,
            EndDate = endDate,
            Note = NormalizeOptional(note),
            ApplyPayload = applyPayload.Trim(),
            SortOrder = sortOrder
        };

        item._intakeTimes.AddRange(intakeTimes);
        return item;
    }

    public void BindToProposal(Guid proposalId)
    {
        if (proposalId == Guid.Empty)
        {
            return;
        }

        if (ProposalId == Guid.Empty || ProposalId == proposalId)
        {
            ProposalId = proposalId;
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private MedicalChangeProposalItem()
    {
    }
}
