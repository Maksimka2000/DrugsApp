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
    /// <summary>
    /// Patient user identifier.
    /// </summary>
    public Guid PatientUserId { get; private set; }

    /// <summary>
    /// Healthcare professional user identifier.
    /// </summary>
    public Guid HealthcareProfessionalUserId { get; private set; }

    /// <summary>
    /// Current state of the supervision request.
    /// </summary>
    public SupervisionRequestState RequestState { get; private set; }

    /// <summary>
    /// UTC timestamp when the request expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the request was created.
    /// </summary>
    public DateTime RequestedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the request was resolved, if any.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>
    /// User identifier that resolved the request, if any.
    /// </summary>
    public Guid? ResolvedByUserId { get; private set; }

    // Factory
    /// <summary>
    /// Creates a new supervision request between a patient and healthcare professional.
    /// </summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="healthcareProfessionalUserId">Healthcare professional user identifier.</param>
    /// <param name="requestedAtUtc">Creation timestamp in UTC.</param>
    /// <returns>Created request or validation errors.</returns>
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
    /// <summary>
    /// Approves the request.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <param name="resolvedByPatientUserId">Patient user who approved.</param>
    /// <returns>Success or relevant error.</returns>
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

    /// <summary>
    /// Rejects the request.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <param name="resolvedByPatientUserId">Patient user who rejected.</param>
    /// <returns>Success or relevant error.</returns>
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

    /// <summary>
    /// Cancels the request by the provider.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <param name="resolvedByProviderUserId">Provider user who cancelled.</param>
    /// <returns>Success or relevant error.</returns>
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

    /// <summary>
    /// Expires the request when its expiry window has passed.
    /// </summary>
    /// <param name="resolvedAtUtc">Resolution timestamp in UTC.</param>
    /// <returns>Success or relevant error.</returns>
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
