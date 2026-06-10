using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.RegularDrugsFeature.Entities;

/// <summary>
/// Owned child row representing one recurring daily intake time for a regular drug schedule.
/// </summary>
public sealed class ClientRegularDrugIntakeTime : BaseEntity
{
    // Attributes

    /// <summary>
    /// Parent regular drug schedule identifier.
    /// </summary>
    public Guid RegularDrugId { get; private set; }

    /// <summary>
    /// Daily intake time unique within the parent schedule.
    /// </summary>
    public TimeOnly IntakeTime { get; private set; }

    // Relationships

    /// <summary>
    /// Parent regular drug schedule navigation.
    /// </summary>
    public ClientRegularDrug RegularDrug { get; private set; } = null!;

    private ClientRegularDrugIntakeTime() : base()
    {
    }

    private ClientRegularDrugIntakeTime(Guid regularDrugId, TimeOnly intakeTime) : this()
    {
        RegularDrugId = regularDrugId;
        IntakeTime = intakeTime;
    }

    /// <summary>
    /// Creates a validated intake-time child row.
    /// </summary>
    public static ErrorOr<ClientRegularDrugIntakeTime> Create(Guid regularDrugId, TimeOnly intakeTime)
    {
        return new ClientRegularDrugIntakeTime(regularDrugId, intakeTime);
    }
}
