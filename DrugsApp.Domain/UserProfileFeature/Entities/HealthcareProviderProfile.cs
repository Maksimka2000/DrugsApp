using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.UserProfileFeature.Entities;

/// <summary>
/// Healthcare provider profile aggregate containing professional details.
/// </summary>
public sealed class HealthcareProviderProfile : BaseEntity
{
    // Attributes

    /// <summary>
    /// Owner user identifier matching the associated user profile.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Optional profession code for the provider.
    /// </summary>
    public HealthcareProviderProfession? ProfessionCode { get; private set; }

    /// <summary>
    /// Custom profession label when <see cref="ProfessionCode"/> is <c>other</c>.
    /// </summary>
    public string? OtherProfessionLabel { get; private set; }

    /// <summary>
    /// Optional workplace label.
    /// </summary>
    public string? WorkplaceLabel { get; private set; }

    /// <summary>
    /// Optional short description displayed on provider card.
    /// </summary>
    public string? ShortDescription { get; private set; }

    /// <summary>
    /// Timestamp when onboarding was completed, if any.
    /// </summary>
    public DateTime? OnboardingCompletedAtUtc { get; private set; }

    // Relationships

    private HealthcareProviderProfile()
    {
    }

    private HealthcareProviderProfile(
        Guid userId,
        HealthcareProviderProfession? professionCode,
        string? otherProfessionLabel,
        string? workplaceLabel,
        string? shortDescription,
        DateTime? onboardingCompletedAtUtc) : this()
    {
        UserId = userId;
        ProfessionCode = professionCode;
        OtherProfessionLabel = otherProfessionLabel;
        WorkplaceLabel = workplaceLabel;
        ShortDescription = shortDescription;
        OnboardingCompletedAtUtc = onboardingCompletedAtUtc;
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a new healthcare provider profile for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="professionCode">Profession code (nullable during onboarding).</param>
    /// <param name="otherProfessionLabel">Custom profession label when profession is other.</param>
    /// <param name="workplaceLabel">Optional workplace label.</param>
    /// <param name="shortDescription">Optional short description.</param>
    /// <returns>Validated profile or errors.</returns>
    public static ErrorOr<HealthcareProviderProfile> Create(
        Guid userId,
        HealthcareProviderProfession? professionCode,
        string? otherProfessionLabel,
        string? workplaceLabel,
        string? shortDescription)
    {
        ErrorOr<ValidatedFields> validated = ValidateAndNormalize(
            userId,
            professionCode,
            otherProfessionLabel,
            workplaceLabel,
            shortDescription);

        if (validated.IsError)
        {
            return validated.Errors;
        }

        return new HealthcareProviderProfile(
            userId,
            validated.Value.ProfessionCode,
            validated.Value.OtherProfessionLabel,
            validated.Value.WorkplaceLabel,
            validated.Value.ShortDescription,
            onboardingCompletedAtUtc: null);
    }

    /// <summary>
    /// Updates provider profile fields.
    /// </summary>
    /// <param name="professionCode">Updated profession code.</param>
    /// <param name="otherProfessionLabel">Updated custom profession label.</param>
    /// <param name="workplaceLabel">Updated workplace label.</param>
    /// <param name="shortDescription">Updated short description.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> Update(
        HealthcareProviderProfession? professionCode,
        string? otherProfessionLabel,
        string? workplaceLabel,
        string? shortDescription)
    {
        ErrorOr<ValidatedFields> validated = ValidateAndNormalize(
            UserId,
            professionCode,
            otherProfessionLabel,
            workplaceLabel,
            shortDescription);

        if (validated.IsError)
        {
            return validated.Errors;
        }

        ProfessionCode = validated.Value.ProfessionCode;
        OtherProfessionLabel = validated.Value.OtherProfessionLabel;
        WorkplaceLabel = validated.Value.WorkplaceLabel;
        ShortDescription = validated.Value.ShortDescription;

        return Result.Success;
    }

    /// <summary>
    /// Marks the provider onboarding as completed.
    /// </summary>
    /// <param name="completedAtUtc">UTC completion timestamp.</param>
    public void MarkOnboardingCompleted(DateTime completedAtUtc)
    {
        OnboardingCompletedAtUtc = completedAtUtc;
    }

    private static ErrorOr<ValidatedFields> ValidateAndNormalize(
        Guid userId,
        HealthcareProviderProfession? professionCode,
        string? otherProfessionLabel,
        string? workplaceLabel,
        string? shortDescription)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "HealthcareProviderProfile.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        if (professionCode is not null && !Enum.IsDefined(typeof(HealthcareProviderProfession), professionCode.Value))
        {
            return Error.Validation(
                code: "HealthcareProviderProfile.ProfessionCodeInvalid",
                description: "Profession code is invalid.",
                metadata: new()
                {
                    ["field"] = "professionCode"
                });
        }

        ErrorOr<string?> normalizedOtherProfessionLabel = NormalizeOptional(otherProfessionLabel, 100, "otherProfessionLabel");
        if (normalizedOtherProfessionLabel.IsError)
        {
            return normalizedOtherProfessionLabel.Errors;
        }

        if (normalizedOtherProfessionLabel.Value is null && professionCode is HealthcareProviderProfession.other)
        {
            return Error.Validation(
                code: "HealthcareProviderProfile.OtherProfessionLabelRequired",
                description: "Other profession label must be provided when profession code is other.",
                metadata: new()
                {
                    ["field"] = "otherProfessionLabel",
                    ["maxLength"] = 100
                });
        }

        ErrorOr<string?> normalizedWorkplaceLabel = NormalizeOptional(workplaceLabel, 150, "workplaceLabel");
        if (normalizedWorkplaceLabel.IsError)
        {
            return normalizedWorkplaceLabel.Errors;
        }

        ErrorOr<string?> normalizedShortDescription = NormalizeOptional(shortDescription, 500, "shortDescription");
        if (normalizedShortDescription.IsError)
        {
            return normalizedShortDescription.Errors;
        }

        return new ValidatedFields(
            professionCode,
            normalizedOtherProfessionLabel.Value,
            normalizedWorkplaceLabel.Value,
            normalizedShortDescription.Value);
    }

    private static ErrorOr<string?> NormalizeOptional(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (string?)null;
        }

        string collapsed = string.Join(
            ' ',
            value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (collapsed.Length > maxLength)
        {
            return Error.Validation(
                code: $"HealthcareProviderProfile.{field}TooLong",
                description: $"{field} must not exceed {maxLength} characters.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = maxLength
                });
        }

        return collapsed;
    }

    private sealed record ValidatedFields(
        HealthcareProviderProfession? ProfessionCode,
        string? OtherProfessionLabel,
        string? WorkplaceLabel,
        string? ShortDescription);
}
