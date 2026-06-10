using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.RegularFoodFeature.Entities;

/// <summary>
/// User-owned recurring food intake schedule referencing a catalog food.
/// </summary>
public sealed class ClientRegularFood : BaseEntity
{
    // Attributes

    /// <summary>
    /// Identity user id of the authenticated client owner.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog food identifier referenced by this schedule.
    /// </summary>
    public Guid FoodId { get; private set; }

    /// <summary>
    /// Persisted lifecycle state for live or archived rows.
    /// </summary>
    public RegularFoodStatus Status { get; private set; }

    /// <summary>
    /// Optional schedule start date; null means active immediately after save.
    /// </summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>
    /// Optional schedule end date after which the row becomes eligible for auto-expire.
    /// </summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Reason the row entered history when status is expired.
    /// </summary>
    public RegularFoodHistoryReason? HistoryReason { get; private set; }

    /// <summary>
    /// UTC timestamp when the row was archived to history.
    /// </summary>
    public DateTime? ArchivedAtUtc { get; private set; }

    // Relationships

    /// <summary>
    /// Catalog food navigation used for read projections.
    /// </summary>
    public Food Food { get; private set; } = null!;

    private readonly List<ClientRegularFoodIntakeTime> _intakeTimes = new();

    /// <summary>
    /// Recurring daily intake times owned by this aggregate.
    /// </summary>
    public IReadOnlyCollection<ClientRegularFoodIntakeTime> IntakeTimes => _intakeTimes.AsReadOnly();

    private ClientRegularFood() : base()
    {
    }

    private ClientRegularFood(
        Guid userId,
        Guid foodId,
        RegularFoodStatus status,
        DateOnly? startDate,
        DateOnly? endDate) : this()
    {
        UserId = userId;
        FoodId = foodId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates a new live regular food schedule for the specified user.
    /// </summary>
    public static ErrorOr<ClientRegularFood> Create(
        Guid userId,
        Guid foodId,
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes,
        DateOnly todayUtcDate)
    {
        ErrorOr<(Guid UserId, Guid FoodId)> identifiers = ValidateIdentifiers(userId, foodId);
        if (identifiers.IsError)
        {
            return identifiers.Errors;
        }

        DateOnly effectiveStartDate = startDate ?? todayUtcDate;

        if (endDate.HasValue && endDate.Value < effectiveStartDate)
        {
            return Error.Validation(
                description: "End date must be greater than or equal to the effective start date.",
                metadata: new()
                {
                    ["field"] = "endDate",
                });
        }

        IReadOnlyList<TimeOnly> normalizedIntakeTimes = NormalizeIntakeTimes(intakeTimes);
        if (HasDuplicateIntakeTimes(normalizedIntakeTimes))
        {
            return Error.Validation(
                description: "Intake times must be unique.",
                metadata: new()
                {
                    ["field"] = "intakeTimes",
                });
        }

        RegularFoodStatus status = ComputeStatus(startDate, todayUtcDate);

        ClientRegularFood aggregate = new(
            userId,
            foodId,
            status,
            startDate,
            endDate);

        ErrorOr<Success> intakeTimesResult = aggregate.ReplaceIntakeTimesInternal(normalizedIntakeTimes);
        if (intakeTimesResult.IsError)
        {
            return intakeTimesResult.Errors;
        }

        return aggregate;
    }

    /// <summary>
    /// Updates schedule dates and intake times on a live row.
    /// </summary>
    public ErrorOr<Success> UpdateSchedule(
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes,
        DateOnly todayUtcDate)
    {
        if (!CanBeEdited())
        {
            return Error.Conflict(
                description: "Regular food schedule is archived and cannot be updated.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        DateOnly effectiveStartDate = (startDate ?? StartDate) ?? todayUtcDate;

        if (endDate.HasValue && endDate.Value < effectiveStartDate)
        {
            return Error.Validation(
                description: "End date must be greater than or equal to the effective start date.",
                metadata: new()
                {
                    ["field"] = "endDate",
                });
        }

        IReadOnlyList<TimeOnly> normalizedIntakeTimes = intakeTimes is null
            ? _intakeTimes.Select(intakeTime => intakeTime.IntakeTime).ToList()
            : NormalizeIntakeTimes(intakeTimes);

        if (HasDuplicateIntakeTimes(normalizedIntakeTimes))
        {
            return Error.Validation(
                description: "Intake times must be unique.",
                metadata: new()
                {
                    ["field"] = "intakeTimes",
                });
        }

        StartDate = startDate ?? StartDate;
        EndDate = endDate ?? EndDate;
        Status = ComputeStatus(StartDate, todayUtcDate);

        return ReplaceIntakeTimesInternal(normalizedIntakeTimes);
    }

    /// <summary>
    /// Archives a live row to history manually.
    /// </summary>
    public ErrorOr<Success> MoveToHistoryManual(DateTime archivedAtUtc)
    {
        if (!CanBeEdited())
        {
            return Error.Conflict(
                description: "Regular food schedule is already archived.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        Status = RegularFoodStatus.Expired;
        HistoryReason = RegularFoodHistoryReason.ManualRemove;
        ArchivedAtUtc = archivedAtUtc;

        return Result.Success;
    }

    /// <summary>
    /// Idempotently archives a live row after its end date has passed.
    /// </summary>
    public ErrorOr<Success> ExpireAutomatically(DateTime archivedAtUtc)
    {
        if (Status == RegularFoodStatus.Expired)
        {
            return Result.Success;
        }

        if (!CanBeEdited() || EndDate is null)
        {
            return Error.Validation(
                description: "Regular food schedule is not eligible for automatic expiration.",
                metadata: new()
                {
                    ["field"] = "status",
                });
        }

        Status = RegularFoodStatus.Expired;
        HistoryReason = RegularFoodHistoryReason.AutoExpire;
        ArchivedAtUtc = archivedAtUtc;

        return Result.Success;
    }

    /// <summary>
    /// Returns whether the row is still editable.
    /// </summary>
    public bool CanBeEdited() =>
        Status is RegularFoodStatus.Scheduled or RegularFoodStatus.Active;

    /// <summary>
    /// Attaches catalog food navigation for response mapping after create.
    /// </summary>
    public void AttachFood(Food food)
    {
        Food = food;
    }

    private ErrorOr<Success> ReplaceIntakeTimesInternal(IReadOnlyList<TimeOnly> intakeTimes)
    {
        _intakeTimes.Clear();

        foreach (TimeOnly intakeTime in intakeTimes.OrderBy(time => time))
        {
            ErrorOr<ClientRegularFoodIntakeTime> child = ClientRegularFoodIntakeTime.Create(Id, intakeTime);
            if (child.IsError)
            {
                return child.Errors;
            }

            _intakeTimes.Add(child.Value);
        }

        return Result.Success;
    }

    private static ErrorOr<(Guid UserId, Guid FoodId)> ValidateIdentifiers(Guid userId, Guid foodId)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                description: "User identifier is required.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        if (foodId == Guid.Empty)
        {
            return Error.Validation(
                description: "Food identifier is required.",
                metadata: new()
                {
                    ["field"] = "foodId",
                });
        }

        return (userId, foodId);
    }

    private static RegularFoodStatus ComputeStatus(DateOnly? startDate, DateOnly todayUtcDate)
    {
        DateOnly effectiveStartDate = startDate ?? todayUtcDate;
        if (effectiveStartDate > todayUtcDate)
        {
            return RegularFoodStatus.Scheduled;
        }

        return RegularFoodStatus.Active;
    }

    private static IReadOnlyList<TimeOnly> NormalizeIntakeTimes(IReadOnlyCollection<TimeOnly>? intakeTimes) =>
        intakeTimes?.ToList() ?? new List<TimeOnly>();

    private static bool HasDuplicateIntakeTimes(IReadOnlyList<TimeOnly> intakeTimes) =>
        intakeTimes.Count != intakeTimes.Distinct().Count();
}
