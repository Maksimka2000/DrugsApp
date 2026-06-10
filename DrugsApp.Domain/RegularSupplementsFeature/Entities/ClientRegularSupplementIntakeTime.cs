using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.RegularSupplementsFeature.Entities;

/// <summary>
/// Owned child row representing one recurring daily intake time for a regular supplement schedule.
/// </summary>
public sealed class ClientRegularSupplementIntakeTime : BaseEntity
{
    // Attributes

    /// <summary>
    /// Parent regular supplement schedule identifier.
    /// </summary>
    public Guid RegularSupplementId { get; private set; }

    /// <summary>
    /// Daily intake time unique within the parent schedule.
    /// </summary>
    public TimeOnly IntakeTime { get; private set; }

    // Relationships

    /// <summary>
    /// Parent regular supplement schedule navigation.
    /// </summary>
    public ClientRegularSupplement RegularSupplement { get; private set; } = null!;

    private ClientRegularSupplementIntakeTime() : base()
    {
    }

    private ClientRegularSupplementIntakeTime(Guid regularSupplementId, TimeOnly intakeTime) : this()
    {
        RegularSupplementId = regularSupplementId;
        IntakeTime = intakeTime;
    }

    /// <summary>
    /// Creates a validated intake-time child row without an assigned parent identifier.
    /// </summary>
    /// <param name="intakeTime">Daily recurring intake clock time.</param>
    /// <returns>Validated child row or validation errors.</returns>
    public static ErrorOr<ClientRegularSupplementIntakeTime> Create(TimeOnly intakeTime)
    {
        return new ClientRegularSupplementIntakeTime(Guid.Empty, intakeTime);
    }

    /// <summary>
    /// Creates a validated intake-time child row for the specified parent schedule.
    /// </summary>
    /// <param name="regularSupplementId">Owning regular supplement identifier.</param>
    /// <param name="intakeTime">Daily recurring intake clock time.</param>
    /// <returns>Validated child row or validation errors.</returns>
    public static ErrorOr<ClientRegularSupplementIntakeTime> Create(Guid regularSupplementId, TimeOnly intakeTime)
    {
        if (regularSupplementId == Guid.Empty)
        {
            return Error.Validation(
                description: "Regular supplement identifier is required.",
                metadata: new()
                {
                    ["field"] = "regularSupplementId",
                });
        }

        return new ClientRegularSupplementIntakeTime(regularSupplementId, intakeTime);
    }
}
