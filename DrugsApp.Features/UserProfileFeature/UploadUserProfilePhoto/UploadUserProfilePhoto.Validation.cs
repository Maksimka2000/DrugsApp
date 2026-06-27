using FluentValidation;

namespace DrugsApp.Features.UserProfileFeature.UploadUserProfilePhoto;

/// <summary>
/// Validator for upload profile photo requests.
/// </summary>
public sealed class UploadUserProfilePhotoRequestValidator : AbstractValidator<UploadUserProfilePhotoRequest>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// Initializes validation rules for profile photo uploads.
    /// </summary>
    public UploadUserProfilePhotoRequestValidator()
    {
        RuleFor(r => r.File)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("File must not be empty.")
            .Must(file => file.Length <= MaxFileSizeBytes)
            .WithMessage("File exceeds maximum size of 5MB.")
            .Must(file => file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only image files are allowed.");
    }
}
