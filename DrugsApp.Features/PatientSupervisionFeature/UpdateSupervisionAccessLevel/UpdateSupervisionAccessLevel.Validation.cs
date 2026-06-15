using DrugsApp.Domain.PatientSupervisionFeature;
using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;

public sealed class UpdateSupervisionAccessLevelRequestValidator : AbstractValidator<UpdateSupervisionAccessLevelRequest>
{
    public UpdateSupervisionAccessLevelRequestValidator()
    {
        RuleFor(x => x.AccessLevelCode)
            .NotEmpty()
            .Must(code => SupervisionAccessLevelCatalog.TryParse(code, out _))
            .WithMessage("access_level_code must be one of view_and_modify|view_only|restricted_view|block_access.");
    }
}
