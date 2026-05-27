using FluentValidation;

namespace DrugsApp.Features.InteractionFeature.UpdateInteraction;

public sealed class UpdateInteractionRequestValidator : AbstractValidator<UpdateInteractionRequest>
{
    public UpdateInteractionRequestValidator()
    {
        RuleFor(request => request.SeverityId)
            .NotEmpty()
            .When(request => request.SeverityId is not null);

        RuleFor(request => request.DefaultText)
            .MaximumLength(4000)
            .When(request => request.DefaultText is not null);

        RuleFor(request => request.StomachText)
            .MaximumLength(4000)
            .When(request => request.StomachText is not null);
    }
}
