using FluentValidation;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

internal sealed class GetDrugToolDetailRequestValidator : AbstractValidator<GetDrugToolDetailRequest>
{
    public GetDrugToolDetailRequestValidator()
    {
        When(
            request => request.SelectedDrugFormId is not null,
            () => RuleFor(request => request.SelectedDrugFormId)
                .Must(id => id != Guid.Empty)
                .WithMessage("selected_drug_form_id must not be empty when provided."));

        RuleFor(request => request.InteractionScope)
            .Must(scope => scope is "all" or "user_only")
            .WithMessage("interaction_scope must be 'all' or 'user_only'.");
    }
}
