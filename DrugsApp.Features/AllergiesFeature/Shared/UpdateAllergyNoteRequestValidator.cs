using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.Shared;

/// <summary>
/// Shared validator for allergy note PATCH requests.
/// </summary>
public sealed class UpdateAllergyNoteRequestValidator : AbstractValidator<UpdateAllergyNoteRequest>
{
    public UpdateAllergyNoteRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.ClearNote == true || request.Note is not null)
            .WithMessage("At least one of note or clearNote must be provided.");

        When(request => !string.IsNullOrWhiteSpace(request.Note), () =>
        {
            RuleFor(request => request.Note!)
                .MaximumLength(1000);
        });
    }
}
