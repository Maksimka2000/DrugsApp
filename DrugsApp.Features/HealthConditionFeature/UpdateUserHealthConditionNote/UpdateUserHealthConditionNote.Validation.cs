using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;

/// <summary>
/// Validator for updating user health condition note.
/// </summary>
public sealed class UpdateUserHealthConditionNoteRequestValidator : AbstractValidator<UpdateUserHealthConditionNoteRequest>
{
    public UpdateUserHealthConditionNoteRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.Note), () =>
        {
            RuleFor(request => request.Note!)
                .MaximumLength(2000);
        });
    }
}
