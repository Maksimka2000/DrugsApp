using DrugsApp.Domain.Common;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Pending or resolved supervision request linking a patient and a healthcare professional.
/// </summary>
public sealed class SupervisionRequest : BaseEntity
{
    // Attributes
    public Guid PatientUserId { get; private set; }
    public Guid HealthcareProfessionalUserId { get; private set; }
    public SupervisionRequestState RequestState { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }

    // Factory
    public static ErrorOr<SupervisionRequest> Create(Guid patientUserId, Guid healthcareProfessionalUserId, DateTime requestedAtUtc)
    {
        if (patientUserId == Guid.Empty || healthcareProfessionalUserId == Guid.Empty || patientUserId == healthcareProfessionalUserId)
        {
            return Error.Validation(
                description: "Invalid supervision request party identifiers.",
                metadata: new Dictionary<string, object>
                {
                    ["patientUserId"] = patientUserId,
                    ["healthcareProfessionalUserId"] = healthcareProfessionalUserId
                });
        }

        var request = new SupervisionRequest
        {
            PatientUserId = patientUserId,
            HealthcareProfessionalUserId = healthcareProfessionalUserId,
            RequestedAtUtc = requestedAtUtc,
            ExpiresAtUtc = requestedAtUtc.AddDays(7),
            RequestState = SupervisionRequestState.Pending
        };

        return request;
    }

    // Behavior
    public ErrorOr<Success> Approve(DateTime resolvedAtUtc, Guid resolvedByPatientUserId)
    {
        ErrorOr<Success> guard = GuardPendingBefore(resolvedAtUtc);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        RequestState = SupervisionRequestState.Approved;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByUserId = resolvedByPatientUserId;
        return Result.Success;
    }

    public ErrorOr<Success> Reject(DateTime resolvedAtUtc, Guid resolvedByPatientUserId)
    {
        ErrorOr<Success> guard = GuardPending(resolvedAtUtc);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        RequestState = SupervisionRequestState.Rejected;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByUserId = resolvedByPatientUserId;
        return Result.Success;
    }

    public ErrorOr<Success> Cancel(DateTime resolvedAtUtc, Guid resolvedByProviderUserId)
    {
        ErrorOr<Success> guard = GuardPending(resolvedAtUtc);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        RequestState = SupervisionRequestState.Cancelled;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByUserId = resolvedByProviderUserId;
        return Result.Success;
    }

    public ErrorOr<Success> Expire(DateTime resolvedAtUtc)
    {
        if (RequestState != SupervisionRequestState.Pending)
        {
            return Error.Conflict(description: "Request already resolved.");
        }

        if (resolvedAtUtc < ExpiresAtUtc)
        {
            return Error.Validation(description: "Cannot expire before scheduled expiry.");
        }

        RequestState = SupervisionRequestState.Expired;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByUserId = null;
        return Result.Success;
    }

    private ErrorOr<Success> GuardPending(DateTime resolvedAtUtc)
    {
        if (RequestState != SupervisionRequestState.Pending)
        {
            return Error.Conflict(description: "Request already resolved.");
        }

        ResolvedAtUtc = resolvedAtUtc;
        return Result.Success;
    }

    private ErrorOr<Success> GuardPendingBefore(DateTime resolvedAtUtc)
    {
        if (RequestState != SupervisionRequestState.Pending)
        {
            return Error.Conflict(description: "Request already resolved.");
        }

        if (resolvedAtUtc > ExpiresAtUtc)
        {
            return Error.Validation(description: "Request expired.");
        }

        ResolvedAtUtc = resolvedAtUtc;
        return Result.Success;
    }

    private SupervisionRequest()
    {
    }
}
