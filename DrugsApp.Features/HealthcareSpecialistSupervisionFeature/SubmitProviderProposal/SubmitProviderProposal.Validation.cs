using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

public sealed class SubmitProviderProposalRequestValidator : AbstractValidator<SubmitProviderProposalRequest>
{
    public SubmitProviderProposalRequestValidator()
    {
        RuleFor(x => x.DraftItems)
            .NotNull()
            .Must(items => items.Count > 0)
            .WithMessage("At least one draft item is required.");

        RuleForEach(x => x.DraftItems)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.CategoryCode)
                    .NotEmpty();

                item.RuleFor(i => i.ApplyPayload)
                    .NotEmpty();
            });
    }
}
