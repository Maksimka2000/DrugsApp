using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.RegularFoodFeature.Entities;

/// <summary>
/// Owned child row representing one recurring daily intake time for a regular food schedule.
/// </summary>
public sealed class ClientRegularFoodIntakeTime : BaseEntity
{
    // Attributes

    /// <summary>
    /// Parent regular food schedule identifier.
    /// </summary>
    public Guid RegularFoodId { get; private set; }

    /// <summary>
    /// Daily intake time unique within the parent schedule.
    /// </summary>
    public TimeOnly IntakeTime { get; private set; }

    // Relationships

    /// <summary>
    /// Parent regular food schedule navigation.
    /// </summary>
    public ClientRegularFood RegularFood { get; private set; } = null!;

    private ClientRegularFoodIntakeTime() : base()
    {
    }

    private ClientRegularFoodIntakeTime(Guid regularFoodId, TimeOnly intakeTime) : this()
    {
        RegularFoodId = regularFoodId;
        IntakeTime = intakeTime;
    }

    /// <summary>
    /// Creates a validated intake-time child row.
    /// </summary>
    public static ErrorOr<ClientRegularFoodIntakeTime> Create(Guid regularFoodId, TimeOnly intakeTime)
    {
        if (regularFoodId == Guid.Empty)
        {
            return Error.Validation(
                description: "Regular food identifier is required.",
                metadata: new()
                {
                    ["field"] = "regularFoodId",
                });
        }

        return new ClientRegularFoodIntakeTime(regularFoodId, intakeTime);
    }
}
