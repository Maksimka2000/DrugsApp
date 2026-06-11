using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectPatientProposal;

public sealed class RejectPatientProposalRequestValidator : AbstractValidator<RejectPatientProposalRequest>
{
    public RejectPatientProposalRequestValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty();
    }
}
