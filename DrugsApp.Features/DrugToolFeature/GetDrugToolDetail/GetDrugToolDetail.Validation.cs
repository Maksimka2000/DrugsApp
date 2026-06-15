using FluentValidation;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

public sealed class GetDrugToolDetailRequestValidator : AbstractValidator<GetDrugToolDetailRequest>
{
    public GetDrugToolDetailRequestValidator()
    {
        RuleFor(request => request.InteractionScope)
            .Must(scope => scope is "all" or "user_only")
            .WithMessage("interaction_scope must be 'all' or 'user_only'.");
    }
}
