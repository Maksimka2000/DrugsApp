using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.RegularSupplementsFeature.Entities;

/// <summary>
/// User-owned recurring supplement intake schedule referencing a catalog supplement or own compound.
/// </summary>
public sealed class ClientRegularSupplement : BaseEntity
{
    // Attributes

    /// <summary>
    /// Authenticated owner identifier scoped to every query and command.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Discriminator for catalog versus own compound schedule targets.
    /// </summary>
    public RegularSupplementSource Source { get; private set; }

    /// <summary>
    /// Catalog supplement identifier when <see cref="Source"/> is <see cref="RegularSupplementSource.Database"/>.
    /// </summary>
    public Guid? SupplementId { get; private set; }

    /// <summary>
    /// Own supplement identifier when <see cref="Source"/> is <see cref="RegularSupplementSource.Own"/>.
    /// </summary>
    public Guid? OwnSupplementId { get; private set; }

    /// <summary>
    /// Effective lifecycle state for live or archived rows.
    /// </summary>
    public RegularSupplementStatus Status { get; private set; }

    /// <summary>
    /// Optional schedule start date; null means active immediately after save.
    /// </summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>
    /// Optional schedule end date after which the row becomes eligible for auto-expire.
    /// </summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Reason the row entered history when <see cref="Status"/> is <see cref="RegularSupplementStatus.Expired"/>.
    /// </summary>
    public RegularSupplementHistoryReason? HistoryReason { get; private set; }

    /// <summary>
    /// UTC timestamp when the row was archived to history.
    /// </summary>
    public DateTime? ArchivedAtUtc { get; private set; }

    // Relationships

    /// <summary>
    /// Catalog supplement navigation used for read projections when <see cref="Source"/> is database-backed.
    /// </summary>
    public Supplement? Supplement { get; private set; }

    /// <summary>
    /// Own supplement navigation used for read projections when <see cref="Source"/> is own-backed.
    /// </summary>
    public ClientOwnSupplement? OwnSupplement { get; private set; }

    private readonly List<ClientRegularSupplementIntakeTime> _intakeTimes = new();

    /// <summary>
    /// Recurring daily intake times owned by this aggregate.
    /// </summary>
    public IReadOnlyCollection<ClientRegularSupplementIntakeTime> IntakeTimes => _intakeTimes.AsReadOnly();

    private ClientRegularSupplement() : base()
    {
    }

    private ClientRegularSupplement(
        Guid userId,
        RegularSupplementSource source,
        Guid? supplementId,
        Guid? ownSupplementId,
        RegularSupplementStatus status,
        DateOnly? startDate,
        DateOnly? endDate) : this()
    {
        UserId = userId;
        Source = source;
        SupplementId = supplementId;
        OwnSupplementId = ownSupplementId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates a new live regular supplement schedule for the specified user.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="source">Catalog or own compound target discriminator.</param>
    /// <param name="supplementId">Catalog supplement identifier when source is database-backed.</param>
    /// <param name="ownSupplementId">Own supplement identifier when source is own-backed.</param>
    /// <param name="startDate">Optional schedule start date.</param>
    /// <param name="endDate">Optional schedule end date.</param>
    /// <param name="intakeTimes">Optional recurring daily intake times.</param>
    /// <param name="today">Current UTC calendar date used for status computation.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<ClientRegularSupplement> Create(
        Guid userId,
        RegularSupplementSource source,
        Guid? supplementId,
        Guid? ownSupplementId,
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes,
        DateOnly today)
    {
        ErrorOr<Success> sourceValidation = ValidateSourceReferences(source, supplementId, ownSupplementId);
        if (sourceValidation.IsError)
        {
            return sourceValidation.Errors;
        }

        if (userId == Guid.Empty)
        {
            return Error.Validation(
                description: "User identifier is required.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
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

        RegularSupplementStatus status = ComputeStatus(startDate, today);

        ClientRegularSupplement aggregate = new(
            userId,
            source,
            supplementId,
            ownSupplementId,
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
    /// <param name="startDate">Optional new schedule start date.</param>
    /// <param name="endDate">Optional new schedule end date.</param>
    /// <param name="intakeTimes">Optional replacement intake times; null keeps existing values.</param>
    /// <param name="today">Current UTC calendar date used for status recomputation.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateSchedule(
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes,
        DateOnly today)
    {
        if (Status == RegularSupplementStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular supplement schedule is archived and cannot be updated.",
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
    /// Attaches catalog navigation for response mapping after create.
    /// </summary>
    /// <param name="supplement">Catalog supplement aggregate.</param>
    public void AttachCatalogReference(Supplement supplement)
    {
        Supplement = supplement;
    }

    /// <summary>
    /// Attaches own supplement navigation for response mapping after create.
    /// </summary>
    /// <param name="ownSupplement">Own supplement aggregate.</param>
    public void AttachOwnSupplementReference(ClientOwnSupplement ownSupplement)
    {
        OwnSupplement = ownSupplement;
    }

    /// <summary>
    /// Transitions a live row to history with the provided reason.
    /// </summary>
    /// <param name="reason">Archive reason.</param>
    /// <param name="archivedAtUtc">UTC archive timestamp.</param>
    /// <returns>Success or conflict errors.</returns>
    public ErrorOr<Success> MoveToHistory(RegularSupplementHistoryReason reason, DateTime archivedAtUtc)
    {
        if (Status == RegularSupplementStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular supplement schedule is already archived.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        Status = RegularSupplementStatus.Expired;
        HistoryReason = reason;
        ArchivedAtUtc = archivedAtUtc;

        return Result.Success;
    }

    /// <summary>
    /// Returns the effective schedule start date used for validation and sorting.
    /// </summary>
    /// <param name="todayUtc">Fallback calendar date when <see cref="StartDate"/> is null.</param>
    /// <returns>Effective start date.</returns>
    public DateOnly ComputeEffectiveStartDate(DateOnly todayUtc) =>
        StartDate ?? DateOnly.FromDateTime(CreatedAtUtc);

    private ErrorOr<Success> ReplaceIntakeTimesInternal(IReadOnlyList<TimeOnly> intakeTimes)
    {
        _intakeTimes.Clear();

        foreach (TimeOnly intakeTime in intakeTimes.OrderBy(time => time))
        {
            ErrorOr<ClientRegularSupplementIntakeTime> child = ClientRegularSupplementIntakeTime.Create(Id, intakeTime);
            if (child.IsError)
            {
                return child.Errors;
            }

            _intakeTimes.Add(child.Value);
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateSourceReferences(
        RegularSupplementSource source,
        Guid? supplementId,
        Guid? ownSupplementId)
    {
        return source switch
        {
            RegularSupplementSource.Database when supplementId is null || supplementId == Guid.Empty =>
                Error.Validation(
                    description: "Supplement identifier is required when source is database-backed.",
                    metadata: new()
                    {
                        ["field"] = "supplementId",
                    }),
            RegularSupplementSource.Database when ownSupplementId is not null =>
                Error.Validation(
                    description: "Own supplement identifier must be null when source is database-backed.",
                    metadata: new()
                    {
                        ["field"] = "ownSupplementId",
                    }),
            RegularSupplementSource.Own when ownSupplementId is null || ownSupplementId == Guid.Empty =>
                Error.Validation(
                    description: "Own supplement identifier is required when source is own-backed.",
                    metadata: new()
                    {
                        ["field"] = "ownSupplementId",
                    }),
            RegularSupplementSource.Own when supplementId is not null =>
                Error.Validation(
                    description: "Supplement identifier must be null when source is own-backed.",
                    metadata: new()
                    {
                        ["field"] = "supplementId",
                    }),
            _ => Result.Success,
        };
    }

    private static RegularSupplementStatus ComputeStatus(DateOnly? startDate, DateOnly today)
    {
        DateOnly effectiveStartDate = startDate ?? today;
        if (effectiveStartDate > today)
        {
            return RegularSupplementStatus.Scheduled;
        }

        return RegularSupplementStatus.Active;
    }

    private static IReadOnlyList<TimeOnly> NormalizeIntakeTimes(IReadOnlyCollection<TimeOnly>? intakeTimes) =>
        intakeTimes?.ToList() ?? new List<TimeOnly>();

    private static bool HasDuplicateIntakeTimes(IReadOnlyList<TimeOnly> intakeTimes) =>
        intakeTimes.Count != intakeTimes.Distinct().Count();
}
