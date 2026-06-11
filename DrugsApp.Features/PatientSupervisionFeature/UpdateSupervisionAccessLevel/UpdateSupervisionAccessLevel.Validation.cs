using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;

public sealed class UpdateSupervisionAccessLevelRequestValidator : AbstractValidator<UpdateSupervisionAccessLevelRequest>
{
    private static readonly string[] AllowedAccessLevels = { "view_and_modify", "view_only", "restricted_view", "block_access" };

    public UpdateSupervisionAccessLevelRequestValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty();

        RuleFor(x => x.AccessLevelCode)
            .NotEmpty()
            .Must(x => AllowedAccessLevels.Contains(x.ToLower()))
            .WithMessage("access_level_code must be one of view_and_modify|view_only|restricted_view|block_access.");
    }
}
