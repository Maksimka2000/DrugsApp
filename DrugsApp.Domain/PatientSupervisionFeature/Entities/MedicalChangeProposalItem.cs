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
    /// <summary>
    /// Identifier of the parent proposal.
    /// </summary>
    public Guid ProposalId { get; private set; }

    /// <summary>
    /// Category of the change being proposed.
    /// </summary>
    public MedicalChangeProposalCategory CategoryCode { get; private set; }

    /// <summary>
    /// Source of the proposal data.
    /// </summary>
    public MedicalChangeProposalSource Source { get; private set; }

    /// <summary>
    /// Optional related catalog identifier.
    /// </summary>
    public Guid? RelatedCatalogId { get; private set; }

    /// <summary>
    /// Optional custom value for the proposed change.
    /// </summary>
    public string? CustomValue { get; private set; }

    /// <summary>
    /// Display name snapshot at submission time.
    /// </summary>
    public string DisplayNameSnapshot { get; private set; } = null!;

    /// <summary>
    /// Optional secondary text snapshot at submission time.
    /// </summary>
    public string? SecondaryTextSnapshot { get; private set; }

    /// <summary>
    /// Optional start date for the change.
    /// </summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>
    /// Optional end date for the change.
    /// </summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Optional note provided with the change.
    /// </summary>
    public string? Note { get; private set; }

    /// <summary>
    /// Serialized payload used to apply the change.
    /// </summary>
    public string ApplyPayload { get; private set; } = null!;

    /// <summary>
    /// Ordering index inside the proposal.
    /// </summary>
    public int SortOrder { get; private set; }

    private readonly List<MedicalChangeProposalItemIntakeTime> _intakeTimes = new();
    /// <summary>
    /// Intake times associated with the proposed item.
    /// </summary>
    public IReadOnlyCollection<MedicalChangeProposalItemIntakeTime> IntakeTimes => _intakeTimes.AsReadOnly();

    // Factory
    /// <summary>
    /// Creates a new medical change proposal item.
    /// </summary>
    /// <param name="proposalId">Parent proposal identifier.</param>
    /// <param name="categoryCode">Proposal category.</param>
    /// <param name="source">Data source of the proposal.</param>
    /// <param name="relatedCatalogId">Optional related catalog identifier.</param>
    /// <param name="customValue">Optional custom value.</param>
    /// <param name="displayNameSnapshot">Display name snapshot.</param>
    /// <param name="secondaryTextSnapshot">Optional secondary text snapshot.</param>
    /// <param name="startDate">Optional start date.</param>
    /// <param name="endDate">Optional end date.</param>
    /// <param name="note">Optional note.</param>
    /// <param name="applyPayload">Serialized payload to apply.</param>
    /// <param name="sortOrder">Order index within the proposal.</param>
    /// <param name="intakeTimes">Associated intake times.</param>
    /// <returns>Created item or validation errors.</returns>
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

    /// <summary>
    /// Binds the item to the provided proposal identifier if not already set.
    /// </summary>
    /// <param name="proposalId">Proposal identifier to bind.</param>
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
