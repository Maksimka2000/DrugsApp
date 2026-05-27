using DrugsApp.Domain.InteractionFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.InteractionFeature.CreateInteraction;

public sealed class CreateInteractionRequestValidator : AbstractValidator<CreateInteractionRequest>
{
    public CreateInteractionRequestValidator()
    {
        RuleFor(request => request.Left)
            .NotNull()
            .SetValidator(new InteractionSideRequestValidator());

        RuleFor(request => request.Right)
            .NotNull()
            .SetValidator(new InteractionSideRequestValidator());

        When(
            request => request.Left is not null && request.Right is not null,
            () =>
            {
                RuleFor(request => request)
                    .Must(request => IsValidCombination(request.Left.Type, request.Right.Type))
                    .WithMessage("interaction_kind must combine substance-family with substance/food/supplement family.");

                RuleFor(request => request)
                    .Must(request => request.Left.Type != request.Right.Type || request.Left.Id != request.Right.Id)
                    .WithMessage("Left and right parties must differ.");
            });

        RuleFor(request => request.SeverityId)
            .NotEmpty();

        RuleFor(request => request.DefaultText)
            .MaximumLength(4000);

        RuleFor(request => request.StomachText)
            .MaximumLength(4000);

        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.DefaultText) || !string.IsNullOrWhiteSpace(request.StomachText))
            .WithMessage("At least one of default_text or stomach_text must be provided.");
    }

    private static bool IsValidCombination(InteractionType leftType, InteractionType rightType)
    {
        bool leftIsSubstanceFamily = IsSubstanceFamily(leftType);
        bool rightIsSubstanceFamily = IsSubstanceFamily(rightType);
        bool rightIsFoodFamily = IsFoodFamily(rightType);
        bool rightIsSupplementFamily = IsSupplementFamily(rightType);

        bool leftIsFoodFamily = IsFoodFamily(leftType);
        bool leftIsSupplementFamily = IsSupplementFamily(leftType);

        return leftIsSubstanceFamily && rightIsSubstanceFamily
               || leftIsSubstanceFamily && rightIsFoodFamily
               || leftIsSubstanceFamily && rightIsSupplementFamily
               || rightIsSubstanceFamily && leftIsFoodFamily
               || rightIsSubstanceFamily && leftIsSupplementFamily
               || rightIsSupplementFamily && leftIsSubstanceFamily;
    }

    private static bool IsSubstanceFamily(InteractionType type) =>
        type is InteractionType.Substance or InteractionType.SubstanceGroup;

    private static bool IsFoodFamily(InteractionType type) =>
        type is InteractionType.Food or InteractionType.FoodGroup;

    private static bool IsSupplementFamily(InteractionType type) =>
        type is InteractionType.Supplement or InteractionType.SupplementGroup;
}

internal sealed class InteractionSideRequestValidator : AbstractValidator<InteractionSideRequest>
{
    public InteractionSideRequestValidator()
    {
        RuleFor(side => side.Type)
            .IsInEnum();

        RuleFor(side => side.Id)
            .NotEmpty();
    }
}
