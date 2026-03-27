using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaFile;

/// <summary>
/// Validator for file upload import request.
/// </summary>
public sealed class UploadImportViaFileRequestValidator : AbstractValidator<UploadImportViaFileRequest>
{
    private const long MaximumFileSizeInBytes = 50 * 1024 * 1024;

    public UploadImportViaFileRequestValidator()
    {
        RuleFor(request => request.File)
            .NotNull()
            .Must(file => file?.Length > 0)
            .WithMessage("File must not be empty.")
            .Must(file => file is not null && file.Length <= MaximumFileSizeInBytes)
            .WithMessage("File must not exceed 50 MB.")
            .Must(file => file is not null && file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only .xlsx files are supported.");
    }
}