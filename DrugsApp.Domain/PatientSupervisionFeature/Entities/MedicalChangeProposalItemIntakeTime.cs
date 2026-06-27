using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Intake time suggestion attached to a proposal item.
/// </summary>
public sealed class MedicalChangeProposalItemIntakeTime : BaseEntity
{
    // Attributes
    /// <summary>
    /// Identifier of the parent proposal item.
    /// </summary>
    public Guid ProposalItemId { get; private set; }

    /// <summary>
    /// Suggested intake time for the item.
    /// </summary>
    public TimeOnly IntakeTime { get; private set; }

    // Factory
    /// <summary>
    /// Creates a new intake time record.
    /// </summary>
    /// <param name="proposalItemId">Parent proposal item identifier.</param>
    /// <param name="intakeTime">Suggested intake time.</param>
    /// <returns>Created intake time record.</returns>
    public static ErrorOr<MedicalChangeProposalItemIntakeTime> Create(Guid proposalItemId, TimeOnly intakeTime)
    {
        var record = new MedicalChangeProposalItemIntakeTime
        {
            ProposalItemId = proposalItemId,
            IntakeTime = intakeTime
        };

        return record;
    }

    /// <summary>
    /// Binds the intake time to the provided proposal item identifier.
    /// </summary>
    /// <param name="proposalItemId">Proposal item identifier.</param>
    public void BindToProposalItem(Guid proposalItemId)
    {
        if (proposalItemId == Guid.Empty)
        {
            return;
        }

        ProposalItemId = proposalItemId;
    }

    private MedicalChangeProposalItemIntakeTime()
    {
    }
}
