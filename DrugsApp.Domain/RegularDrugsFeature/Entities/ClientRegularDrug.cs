using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.RegularDrugsFeature.Entities;

/// <summary>
/// User-owned recurring drug intake schedule referencing a catalog drug and form.
/// </summary>
public sealed class ClientRegularDrug : BaseEntity
{
    // Attributes

    /// <summary>
    /// Authenticated owner identifier scoped to every query and command.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog drug identifier referenced by this schedule.
    /// </summary>
    public Guid DrugId { get; private set; }

    /// <summary>
    /// Catalog drug form identifier selected for this schedule.
    /// </summary>
    public Guid DrugFormId { get; private set; }

    /// <summary>
    /// Effective lifecycle state for live or archived rows.
    /// </summary>
    public RegularDrugStatus Status { get; private set; }

    /// <summary>
    /// Optional schedule start date; null means active immediately after save.
    /// </summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>
    /// Optional schedule end date after which the row becomes eligible for auto-expire.
    /// </summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Reason the row entered history when <see cref="Status"/> is <see cref="RegularDrugStatus.Expired"/>.
    /// </summary>
    public RegularDrugHistoryReason? HistoryReason { get; private set; }

    /// <summary>
    /// UTC timestamp when the row was archived to history.
    /// </summary>
    public DateTime? ArchivedAtUtc { get; private set; }

    // Relationships

    /// <summary>
    /// Catalog drug navigation used for read projections.
    /// </summary>
    public Drug Drug { get; private set; } = null!;

    /// <summary>
    /// Catalog drug form navigation used for read projections.
    /// </summary>
    public DrugForm DrugForm { get; private set; } = null!;

    private readonly List<ClientRegularDrugIntakeTime> _intakeTimes = new();

    /// <summary>
    /// Recurring daily intake times owned by this aggregate.
    /// </summary>
    public IReadOnlyCollection<ClientRegularDrugIntakeTime> IntakeTimes => _intakeTimes.AsReadOnly();

    private ClientRegularDrug() : base()
    {
    }

    private ClientRegularDrug(
        Guid userId,
        Guid drugId,
        Guid drugFormId,
        RegularDrugStatus status,
        DateOnly? startDate,
        DateOnly? endDate) : this()
    {
        UserId = userId;
        DrugId = drugId;
        DrugFormId = drugFormId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates a new live regular drug schedule for the specified user.
    /// </summary>
    public static ErrorOr<ClientRegularDrug> Create(
        Guid userId,
        Guid drugId,
        Guid drugFormId,
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes,
        DateOnly today)
    {
        ErrorOr<(Guid UserId, Guid DrugId, Guid DrugFormId)> identifiers = ValidateIdentifiers(userId, drugId, drugFormId);
        if (identifiers.IsError)
        {
            return identifiers.Errors;
        }

        DateOnly effectiveStartDate = startDate ?? today;

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

        RegularDrugStatus status = ComputeStatus(startDate, today);

        ClientRegularDrug aggregate = new(
            userId,
            drugId,
            drugFormId,
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
        DateOnly today)
    {
        if (Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is archived and cannot be updated.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        DateOnly effectiveStartDate = (startDate ?? StartDate) ?? today;

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
        Status = ComputeStatus(StartDate, today);

        return ReplaceIntakeTimesInternal(normalizedIntakeTimes);
    }

    /// <summary>
    /// Attaches catalog navigations for response mapping after create.
    /// </summary>
    public void AttachCatalogReferences(Drug drug, DrugForm drugForm)
    {
        Drug = drug;
        DrugForm = drugForm;
    }

    /// <summary>
    /// Transitions a live row to history with the provided reason.
    /// </summary>
    public ErrorOr<Success> MoveToHistory(RegularDrugHistoryReason reason, DateTime archivedAtUtc)
    {
        if (Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is already archived.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        Status = RegularDrugStatus.Expired;
        HistoryReason = reason;
        ArchivedAtUtc = archivedAtUtc;

        return Result.Success;
    }

    /// <summary>
    /// Replaces owned intake-time children with a validated unique set.
    /// </summary>
    public ErrorOr<Success> ReplaceIntakeTimes(IReadOnlyCollection<TimeOnly> intakeTimes)
    {
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

        return ReplaceIntakeTimesInternal(normalizedIntakeTimes);
    }

    private ErrorOr<Success> ReplaceIntakeTimesInternal(IReadOnlyList<TimeOnly> intakeTimes)
    {
        _intakeTimes.Clear();

        foreach (TimeOnly intakeTime in intakeTimes.OrderBy(time => time))
        {
            ErrorOr<ClientRegularDrugIntakeTime> child = ClientRegularDrugIntakeTime.Create(Id, intakeTime);
            if (child.IsError)
            {
                return child.Errors;
            }

            _intakeTimes.Add(child.Value);
        }

        return Result.Success;
    }

    private static ErrorOr<(Guid UserId, Guid DrugId, Guid DrugFormId)> ValidateIdentifiers(
        Guid userId,
        Guid drugId,
        Guid drugFormId)
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

        if (drugId == Guid.Empty)
        {
            return Error.Validation(
                description: "Drug identifier is required.",
                metadata: new()
                {
                    ["field"] = "drugId",
                });
        }

        if (drugFormId == Guid.Empty)
        {
            return Error.Validation(
                description: "Drug form identifier is required.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                });
        }

        return (userId, drugId, drugFormId);
    }

    private static RegularDrugStatus ComputeStatus(DateOnly? startDate, DateOnly today)
    {
        DateOnly effectiveStartDate = startDate ?? today;
        if (effectiveStartDate > today)
        {
            return RegularDrugStatus.Scheduled;
        }

        return RegularDrugStatus.Active;
    }

    private static IReadOnlyList<TimeOnly> NormalizeIntakeTimes(IReadOnlyCollection<TimeOnly>? intakeTimes) =>
        intakeTimes?.ToList() ?? new List<TimeOnly>();

    private static bool HasDuplicateIntakeTimes(IReadOnlyList<TimeOnly> intakeTimes) =>
        intakeTimes.Count != intakeTimes.Distinct().Count();
}
