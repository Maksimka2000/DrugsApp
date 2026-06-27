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
    /// <summary>
    /// Identifier of the supervision connection the proposal belongs to.
    /// </summary>
    public Guid SupervisionConnectionId { get; private set; }

    /// <summary>
    /// Patient user identifier.
    /// </summary>
    public Guid PatientUserId { get; private set; }

    /// <summary>
    /// Healthcare professional user identifier.
    /// </summary>
    public Guid HealthcareProfessionalUserId { get; private set; }

    /// <summary>
    /// Current state of the proposal.
    /// </summary>
    public MedicalChangeProposalState ProposalState { get; private set; }

    /// <summary>
    /// Access level snapshot captured at submission time.
    /// </summary>
    public SupervisionAccessLevel AccessLevelSnapshot { get; private set; }

    /// <summary>
    /// Short summary describing the proposal content.
    /// </summary>
    public string ProposalSummary { get; private set; } = null!;

    /// <summary>
    /// UTC timestamp when the proposal was submitted.
    /// </summary>
    public DateTime SubmittedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the proposal was resolved, if any.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>
    /// Patient user identifier who resolved the proposal, when applicable.
    /// </summary>
    public Guid? ResolvedByPatientUserId { get; private set; }

    private readonly List<MedicalChangeProposalItem> _items = new();
    /// <summary>
    /// Proposal items requested by the healthcare professional.
    /// </summary>
    public IReadOnlyCollection<MedicalChangeProposalItem> Items => _items.AsReadOnly();

    // Factory
    /// <summary>
    /// Creates a new medical change proposal.
    /// </summary>
    /// <param name="connectionId">Supervision connection identifier.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="healthcareProfessionalUserId">Healthcare professional user identifier.</param>
    /// <param name="accessLevelSnapshot">Access level snapshot at submission.</param>
    /// <param name="proposalSummary">Short description of the proposal.</param>
    /// <param name="submittedAtUtc">Submission timestamp in UTC.</param>
    /// <param name="items">Requested proposal items.</param>
    /// <returns>Created proposal or validation errors.</returns>
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
    /// <summary>
    /// Approves the proposal.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <param name="resolvedByPatientUserId">Patient user who approved.</param>
    /// <returns>Success or conflict error.</returns>
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

    /// <summary>
    /// Rejects the proposal.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <param name="resolvedByPatientUserId">Patient user who rejected.</param>
    /// <returns>Success or conflict error.</returns>
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

    /// <summary>
    /// Invalidates the proposal without approval or rejection.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <returns>Success or conflict error.</returns>
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
