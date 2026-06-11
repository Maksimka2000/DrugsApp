using DrugsApp.Domain.Common;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Entities;

/// <summary>
/// Approved supervision connection with patient-controlled access level.
/// </summary>
public sealed class SupervisionConnection : BaseEntity
{
    // Attributes
    public Guid PatientUserId { get; private set; }
    public Guid HealthcareProfessionalUserId { get; private set; }
    public SupervisionAccessLevel AccessLevelCode { get; private set; }
    public SupervisionConnectionState ConnectionState { get; private set; }
    public DateTime ApprovedAtUtc { get; private set; }
    public DateTime? RemovedAtUtc { get; private set; }

    // Factory
    public static ErrorOr<SupervisionConnection> CreateOrReactivate(
        Guid patientUserId,
        Guid healthcareProfessionalUserId,
        SupervisionAccessLevel accessLevelCode,
        DateTime approvedAtUtc)
    {
        if (patientUserId == Guid.Empty || healthcareProfessionalUserId == Guid.Empty || patientUserId == healthcareProfessionalUserId)
        {
            return Error.Validation(
                description: "Invalid supervision connection party identifiers.",
                metadata: new Dictionary<string, object>
                {
                    ["patientUserId"] = patientUserId,
                    ["healthcareProfessionalUserId"] = healthcareProfessionalUserId
                });
        }

        var connection = new SupervisionConnection
        {
            PatientUserId = patientUserId,
            HealthcareProfessionalUserId = healthcareProfessionalUserId,
            AccessLevelCode = accessLevelCode,
            ConnectionState = SupervisionConnectionState.Active,
            ApprovedAtUtc = approvedAtUtc,
            RemovedAtUtc = null
        };

        return connection;
    }

    // Behavior
    public ErrorOr<Success> Reactivate(SupervisionAccessLevel accessLevelCode, DateTime approvedAtUtc)
    {
        if (ConnectionState != SupervisionConnectionState.Removed)
        {
            return Error.Conflict(description: "Connection is not removed.");
        }

        AccessLevelCode = accessLevelCode;
        ConnectionState = SupervisionConnectionState.Active;
        ApprovedAtUtc = approvedAtUtc;
        RemovedAtUtc = null;
        UpdatedAtUtc = approvedAtUtc;
        return Result.Success;
    }

    public ErrorOr<Success> UpdateAccessLevel(SupervisionAccessLevel accessLevelCode, DateTime updatedAtUtc)
    {
        if (ConnectionState != SupervisionConnectionState.Active)
        {
            return Error.Conflict(description: "Connection is not active.");
        }

        AccessLevelCode = accessLevelCode;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success;
    }

    public ErrorOr<Success> Remove(DateTime removedAtUtc)
    {
        if (ConnectionState != SupervisionConnectionState.Active)
        {
            return Error.Conflict(description: "Connection already removed.");
        }

        ConnectionState = SupervisionConnectionState.Removed;
        RemovedAtUtc = removedAtUtc;
        return Result.Success;
    }

    private SupervisionConnection()
    {
    }
}
