using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.MealTimeManagementFeature.Entities;

/// <summary>
/// User-owned meal label and recurring daily intake time record.
/// </summary>
public sealed class ClientMealTime : BaseEntity
{
    // Attributes

    /// <summary>
    /// Authenticated owner identifier derived from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// User-entered meal label stored in trimmed display form.
    /// </summary>
    public string MealName { get; private set; } = null!;

    /// <summary>
    /// Upper-invariant normalized meal name used for per-user uniqueness checks.
    /// </summary>
    public string NormalizedMealName { get; private set; } = null!;

    /// <summary>
    /// Recurring daily time-of-day for the meal intake.
    /// </summary>
    public TimeOnly IntakeTime { get; private set; }

    private ClientMealTime() : base()
    {
    }

    private ClientMealTime(
        Guid userId,
        string mealName,
        string normalizedMealName,
        TimeOnly intakeTime) : this()
    {
        UserId = userId;
        MealName = mealName;
        NormalizedMealName = normalizedMealName;
        IntakeTime = intakeTime;
    }

    /// <summary>
    /// Creates a new meal time record for the authenticated user.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="mealName">Meal label.</param>
    /// <param name="intakeTime">Daily intake time.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<ClientMealTime> Create(Guid userId, string mealName, TimeOnly intakeTime)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "MealTime.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        ErrorOr<(string DisplayValue, string NormalizedValue)> normalizedName = NormalizeMealName(mealName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        return new ClientMealTime(
            userId: userId,
            mealName: normalizedName.Value.DisplayValue,
            normalizedMealName: normalizedName.Value.NormalizedValue,
            intakeTime: intakeTime);
    }

    /// <summary>
    /// Updates the meal label while preserving normalized uniqueness semantics.
    /// </summary>
    /// <param name="mealName">New meal label.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateMealName(string mealName)
    {
        ErrorOr<(string DisplayValue, string NormalizedValue)> normalizedName = NormalizeMealName(mealName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        if (normalizedName.Value.NormalizedValue.Equals(NormalizedMealName, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        MealName = normalizedName.Value.DisplayValue;
        NormalizedMealName = normalizedName.Value.NormalizedValue;
        return Result.Success;
    }

    /// <summary>
    /// Updates the recurring daily intake time.
    /// </summary>
    /// <param name="intakeTime">New intake time.</param>
    /// <returns>Success.</returns>
    public ErrorOr<Success> UpdateIntakeTime(TimeOnly intakeTime)
    {
        if (intakeTime == IntakeTime)
        {
            return Result.Success;
        }

        IntakeTime = intakeTime;
        return Result.Success;
    }

    private static ErrorOr<(string DisplayValue, string NormalizedValue)> NormalizeMealName(string value)
    {
        string trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Error.Validation(
                code: "MealTime.MealNameEmpty",
                description: "Meal name must be provided.",
                metadata: new()
                {
                    ["field"] = "mealName"
                });
        }

        if (trimmed.Length > 128)
        {
            return Error.Validation(
                code: "MealTime.MealNameTooLong",
                description: "Meal name must not exceed 128 characters.",
                metadata: new()
                {
                    ["field"] = "mealName",
                    ["maxLength"] = 128
                });
        }

        return (trimmed, trimmed.ToUpperInvariant());
    }
}
