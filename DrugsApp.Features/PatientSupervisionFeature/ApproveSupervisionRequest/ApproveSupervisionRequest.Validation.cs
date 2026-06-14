using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.ApproveSupervisionRequest;

public sealed class ApproveSupervisionRequestRequestValidator : AbstractValidator<ApproveSupervisionRequestRequest>
{
    private static readonly string[] AllowedAccessLevels = { "ViewAndModify", "ViewOnly", "RestrictedView", "BlockAccess" };

    public ApproveSupervisionRequestRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty();

        RuleFor(x => x.AccessLevelCode)
            .NotEmpty()
            .Must(x => AllowedAccessLevels.Contains(x.ToLower()))
            .WithMessage("access_level_code must be one of ViewAndModify|ViewOnly|RestrictedView|BlockAccess.");
    }
}
