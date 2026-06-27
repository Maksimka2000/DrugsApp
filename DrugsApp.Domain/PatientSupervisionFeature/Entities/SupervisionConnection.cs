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
    /// <summary>
    /// Patient user identifier.
    /// </summary>
    public Guid PatientUserId { get; private set; }

    /// <summary>
    /// Healthcare professional user identifier.
    /// </summary>
    public Guid HealthcareProfessionalUserId { get; private set; }

    /// <summary>
    /// Patient-controlled access level for this connection.
    /// </summary>
    public SupervisionAccessLevel AccessLevelCode { get; private set; }

    /// <summary>
    /// Current connection state.
    /// </summary>
    public SupervisionConnectionState ConnectionState { get; private set; }

    /// <summary>
    /// UTC timestamp when the connection was approved.
    /// </summary>
    public DateTime ApprovedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the connection was removed, if applicable.
    /// </summary>
    public DateTime? RemovedAtUtc { get; private set; }

    // Factory
    /// <summary>
    /// Creates or reactivates a supervision connection.
    /// </summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="healthcareProfessionalUserId">Healthcare professional user identifier.</param>
    /// <param name="accessLevelCode">Requested access level.</param>
    /// <param name="approvedAtUtc">Approval timestamp in UTC.</param>
    /// <returns>Active supervision connection or validation errors.</returns>
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
    /// <summary>
    /// Reactivates a removed connection with a new access level.
    /// </summary>
    /// <param name="accessLevelCode">Access level to apply.</param>
    /// <param name="approvedAtUtc">Approval timestamp in UTC.</param>
    /// <returns>Success or conflict error.</returns>
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

    /// <summary>
    /// Updates the access level for an active connection.
    /// </summary>
    /// <param name="accessLevelCode">Access level to apply.</param>
    /// <param name="updatedAtUtc">Update timestamp in UTC.</param>
    /// <returns>Success or conflict error.</returns>
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

    /// <summary>
    /// Removes the connection.
    /// </summary>
    /// <param name="removedAtUtc">Removal timestamp in UTC.</param>
    /// <returns>Success or conflict error.</returns>
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
