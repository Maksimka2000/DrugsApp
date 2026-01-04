using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;

/// <summary>
/// Validator for UpdateTempDrugStatus request payload.
/// </summary>
public sealed class UpdateTempDrugStatusRequestValidator : AbstractValidator<UpdateTempDrugStatusRequest>
{
    public UpdateTempDrugStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid status value.");

        When(
            x => x.Note is not null,
            () => RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage($"Note must not exceed {500} characters."));
    }
}


