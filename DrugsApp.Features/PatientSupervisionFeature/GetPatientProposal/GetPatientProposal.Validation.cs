using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

public sealed class GetPatientProposalRequestValidator : AbstractValidator<GetPatientProposalRequest>
{
    public GetPatientProposalRequestValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty();
    }
}
