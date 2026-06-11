using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.ApprovePatientProposal;

public sealed class ApprovePatientProposalRequestValidator : AbstractValidator<ApprovePatientProposalRequest>
{
    public ApprovePatientProposalRequestValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty();
    }
}
