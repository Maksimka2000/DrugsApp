using DrugsApp.Domain.Common;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Provider-submitted add-only proposal awaiting patient approval.
/// </summary>
public sealed class MedicalChangeProposal : BaseEntity
{
    // Attributes
    public Guid SupervisionConnectionId { get; private set; }
    public Guid PatientUserId { get; private set; }
    public Guid HealthcareProfessionalUserId { get; private set; }
    public MedicalChangeProposalState ProposalState { get; private set; }
    public SupervisionAccessLevel AccessLevelSnapshot { get; private set; }
    public string ProposalSummary { get; private set; } = null!;
    public DateTime SubmittedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public Guid? ResolvedByPatientUserId { get; private set; }

    private readonly List<MedicalChangeProposalItem> _items = new();
    public IReadOnlyCollection<MedicalChangeProposalItem> Items => _items.AsReadOnly();

    // Factory
    public static ErrorOr<MedicalChangeProposal> Create(
        Guid connectionId,
        Guid patientUserId,
        Guid healthcareProfessionalUserId,
        SupervisionAccessLevel accessLevelSnapshot,
        string proposalSummary,
        DateTime submittedAtUtc,
        IReadOnlyCollection<MedicalChangeProposalItem> items)
    {
        if (accessLevelSnapshot != SupervisionAccessLevel.ViewAndModify)
        {
            return Error.Validation(description: "Access level must allow modify.");
        }

        if (string.IsNullOrWhiteSpace(proposalSummary))
        {
            return Error.Validation(description: "Proposal summary is required.");
        }

        var proposal = new MedicalChangeProposal
        {
            SupervisionConnectionId = connectionId,
            PatientUserId = patientUserId,
            HealthcareProfessionalUserId = healthcareProfessionalUserId,
            AccessLevelSnapshot = accessLevelSnapshot,
            ProposalSummary = proposalSummary.Trim(),
            SubmittedAtUtc = submittedAtUtc,
            ProposalState = MedicalChangeProposalState.Pending
        };

        proposal._items.AddRange(items);
        return proposal;
    }

    // Behavior
    public ErrorOr<Success> Approve(DateTime resolvedAtUtc, Guid resolvedByPatientUserId)
    {
        if (ProposalState != MedicalChangeProposalState.Pending)
        {
            return Error.Conflict(description: "Proposal already resolved.");
        }

        ProposalState = MedicalChangeProposalState.Approved;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByPatientUserId = resolvedByPatientUserId;
        return Result.Success;
    }

    public ErrorOr<Success> Reject(DateTime resolvedAtUtc, Guid resolvedByPatientUserId)
    {
        if (ProposalState != MedicalChangeProposalState.Pending)
        {
            return Error.Conflict(description: "Proposal already resolved.");
        }

        ProposalState = MedicalChangeProposalState.Rejected;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByPatientUserId = resolvedByPatientUserId;
        return Result.Success;
    }

    public ErrorOr<Success> Invalidate(DateTime resolvedAtUtc)
    {
        if (ProposalState != MedicalChangeProposalState.Pending)
        {
            return Error.Conflict(description: "Proposal already resolved.");
        }

        ProposalState = MedicalChangeProposalState.Invalidated;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByPatientUserId = null;
        return Result.Success;
    }

    private MedicalChangeProposal()
    {
    }
}
