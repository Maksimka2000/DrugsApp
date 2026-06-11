using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Intake time suggestion attached to a proposal item.
/// </summary>
public sealed class MedicalChangeProposalItemIntakeTime : BaseEntity
{
    // Attributes
    public Guid ProposalItemId { get; private set; }
    public TimeOnly IntakeTime { get; private set; }

    // Factory
    public static ErrorOr<MedicalChangeProposalItemIntakeTime> Create(Guid proposalItemId, TimeOnly intakeTime)
    {
        var record = new MedicalChangeProposalItemIntakeTime
        {
            ProposalItemId = proposalItemId,
            IntakeTime = intakeTime
        };

        return record;
    }

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
