using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaUrl;

/// <summary>
/// Validator for URL upload import request.
/// </summary>
public sealed class UploadImportViaUrlRequestValidator : AbstractValidator<UploadImportViaUrlRequest>
{
    public UploadImportViaUrlRequestValidator()
    {
        RuleFor(request => request.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeHttpOrHttpsAbsoluteUri)
            .WithMessage("Url must be a valid absolute HTTP or HTTPS URL.");
    }

    private static bool BeHttpOrHttpsAbsoluteUri(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme is "http" or "https";
    }
}

