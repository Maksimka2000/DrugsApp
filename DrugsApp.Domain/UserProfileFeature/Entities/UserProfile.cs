using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.UserProfileFeature.Entities;

/// <summary>
/// Primary user profile aggregate mirrored to the authenticated identity user.
/// </summary>
public sealed class UserProfile : BaseEntity
{
    // Attributes

    /// <summary>
    /// Active login email mirrored from Identity.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// User first name stored for profile presentation.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// User last name stored for profile presentation.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// User date of birth used for eligibility checks.
    /// </summary>
    public DateOnly DateOfBirth { get; private set; }

    /// <summary>
    /// Gender selection persisted as a string in the database.
    /// </summary>
    public ProfileGender Gender { get; private set; }

    /// <summary>
    /// Storage key for the processed profile photo blob. Null when not set.
    /// </summary>
    public string? ProfilePhotoStorageKey { get; private set; }

    /// <summary>
    /// UTC timestamp when the client completed patient onboarding. Null until marked complete.
    /// </summary>
    public DateTime? OnboardingCompletedAtUtc { get; private set; }

    // Relationships (aggregate root)

    private UserProfile()
    {
    }

    private UserProfile(
        Guid id,
        string email,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        ProfileGender gender) : this()
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a new user profile aggregate with validated personal data.
    /// </summary>
    /// <param name="id">Identity user id shared with the profile.</param>
    /// <param name="email">Normalized active login email.</param>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <param name="dateOfBirth">Date of birth (must not be in the future).</param>
    /// <param name="gender">Gender selection.</param>
    /// <returns>Validated <see cref="UserProfile"/> or validation errors.</returns>
    public static ErrorOr<UserProfile> Create(
        Guid id,
        string email,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        ProfileGender gender)
    {
        if (id == Guid.Empty)
        {
            return Error.Validation(
                code: "UserProfile.IdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "id"
                });
        }

        ErrorOr<string> normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.IsError)
        {
            return normalizedEmail.Errors;
        }

        ErrorOr<string> normalizedFirstName = NormalizeName(firstName, "firstName");
        if (normalizedFirstName.IsError)
        {
            return normalizedFirstName.Errors;
        }

        ErrorOr<string> normalizedLastName = NormalizeName(lastName, "lastName");
        if (normalizedLastName.IsError)
        {
            return normalizedLastName.Errors;
        }

        ErrorOr<DateOnly> validatedDateOfBirth = ValidateDateOfBirth(dateOfBirth);
        if (validatedDateOfBirth.IsError)
        {
            return validatedDateOfBirth.Errors;
        }

        return new UserProfile(
            id,
            normalizedEmail.Value,
            normalizedFirstName.Value,
            normalizedLastName.Value,
            validatedDateOfBirth.Value,
            gender);
    }

    /// <summary>
    /// Updates personal data for the profile.
    /// </summary>
    /// <param name="firstName">New first name.</param>
    /// <param name="lastName">New last name.</param>
    /// <param name="dateOfBirth">New date of birth.</param>
    /// <param name="gender">New gender selection.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdatePersonalData(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        ProfileGender gender)
    {
        ErrorOr<string> normalizedFirstName = NormalizeName(firstName, "firstName");
        if (normalizedFirstName.IsError)
        {
            return normalizedFirstName.Errors;
        }

        ErrorOr<string> normalizedLastName = NormalizeName(lastName, "lastName");
        if (normalizedLastName.IsError)
        {
            return normalizedLastName.Errors;
        }

        ErrorOr<DateOnly> validatedDateOfBirth = ValidateDateOfBirth(dateOfBirth);
        if (validatedDateOfBirth.IsError)
        {
            return validatedDateOfBirth.Errors;
        }

        FirstName = normalizedFirstName.Value;
        LastName = normalizedLastName.Value;
        DateOfBirth = validatedDateOfBirth.Value;
        Gender = gender;

        return Result.Success;
    }

    /// <summary>
    /// Sets the storage key for the profile photo after upload.
    /// </summary>
    /// <param name="storageKey">Blob storage key.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> SetProfilePhotoStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return Error.Validation(
                code: "UserProfile.ProfilePhotoStorageKeyEmpty",
                description: "Profile photo storage key must be provided.",
                metadata: new()
                {
                    ["field"] = "profilePhotoStorageKey"
                });
        }

        string normalized = storageKey.Trim();
        if (normalized.Length > 64)
        {
            return Error.Validation(
                code: "UserProfile.ProfilePhotoStorageKeyTooLong",
                description: "Profile photo storage key must not exceed 64 characters.",
                metadata: new()
                {
                    ["field"] = "profilePhotoStorageKey",
                    ["maxLength"] = 64
                });
        }

        if (normalized.Equals(ProfilePhotoStorageKey, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        ProfilePhotoStorageKey = normalized;
        return Result.Success;
    }

    /// <summary>
    /// Removes the profile photo reference.
    /// </summary>
    public void RemoveProfilePhoto()
    {
        ProfilePhotoStorageKey = null;
    }

    /// <summary>
    /// Marks the client onboarding as completed.
    /// </summary>
    /// <param name="completedAtUtc">UTC completion timestamp.</param>
    public void MarkOnboardingCompleted(DateTime completedAtUtc)
    {
        OnboardingCompletedAtUtc = completedAtUtc;
    }

    /// <summary>
    /// Updates mirrored email after confirmation.
    /// </summary>
    /// <param name="email">Normalized email value.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateEmail(string email)
    {
        ErrorOr<string> normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.IsError)
        {
            return normalizedEmail.Errors;
        }

        if (normalizedEmail.Value.Equals(Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success;
        }

        Email = normalizedEmail.Value;
        return Result.Success;
    }

    private static ErrorOr<string> NormalizeEmail(string value)
    {
        string trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Error.Validation(
                code: "UserProfile.EmailEmpty",
                description: "Email must be provided.",
                metadata: new()
                {
                    ["field"] = "email"
                });
        }

        if (trimmed.Length > 256)
        {
            return Error.Validation(
                code: "UserProfile.EmailTooLong",
                description: "Email must not exceed 256 characters.",
                metadata: new()
                {
                    ["field"] = "email",
                    ["maxLength"] = 256
                });
        }

        string normalized = trimmed.ToLowerInvariant();
        return normalized;
    }

    private static ErrorOr<string> NormalizeName(string value, string field)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: $"UserProfile.{field}Empty",
                description: $"{field} must be provided.",
                metadata: new()
                {
                    ["field"] = field
                });
        }

        if (collapsed.Length > 100)
        {
            return Error.Validation(
                code: $"UserProfile.{field}TooLong",
                description: $"{field} must not exceed 100 characters.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = 100
                });
        }

        return collapsed;
    }

    private static ErrorOr<DateOnly> ValidateDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dateOfBirth > today)
        {
            return Error.Validation(
                code: "UserProfile.DateOfBirthInFuture",
                description: "Date of birth must not be in the future.",
                metadata: new()
                {
                    ["field"] = "dateOfBirth"
                });
        }

        return dateOfBirth;
    }
}
