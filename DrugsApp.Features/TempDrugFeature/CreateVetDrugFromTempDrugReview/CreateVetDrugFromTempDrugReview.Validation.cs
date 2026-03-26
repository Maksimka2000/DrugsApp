using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;

public sealed class CreateVetDrugFromTempDrugReviewRequestValidator : AbstractValidator<CreateVetDrugFromTempDrugReviewRequest>
{
    public CreateVetDrugFromTempDrugReviewRequestValidator()
    {
        RuleFor(x => x.CommercialName)
            .NotEmpty()
            .WithMessage("CommercialName is required.")
            .MaximumLength(300)
            .WithMessage("CommercialName must not exceed 300 characters.");

        RuleFor(x => x.TypeOfProduct)
            .NotEmpty()
            .WithMessage("TypeOfProduct is required.")
            .MaximumLength(100)
            .WithMessage("TypeOfProduct must not exceed 100 characters.");

        RuleFor(x => x.ProductIdentifier)
            .NotEmpty()
            .WithMessage("ProductIdentifier is required.")
            .MaximumLength(64)
            .WithMessage("ProductIdentifier must not exceed 64 characters.")
            .Must(value => int.TryParse(value, out int parsed) && parsed > 0)
            .WithMessage("ProductIdentifier must be a positive integer.");

        When(x => x.Note is not null, () => RuleFor(x => x.Note)
            .MaximumLength(2000)
            .WithMessage("Note must not exceed 2000 characters."));
    }
}

