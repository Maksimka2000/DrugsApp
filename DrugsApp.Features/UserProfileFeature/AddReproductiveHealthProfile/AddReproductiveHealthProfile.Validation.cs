using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;

public sealed class AddReproductiveHealthProfileRequestValidator : AbstractValidator<AddReproductiveHealthProfileRequest>
{
    public AddReproductiveHealthProfileRequestValidator()
    {
        RuleFor(r => r.ContraceptionMethodCode)
            .Must(code => code is null || Enum.IsDefined(typeof(ContraceptionMethodCode), code.Value))
            .WithMessage("Invalid contraception method code.");
    }
}
