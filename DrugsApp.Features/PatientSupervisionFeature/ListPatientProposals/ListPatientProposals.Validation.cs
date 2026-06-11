using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

public sealed class ListPatientProposalsRequestValidator : AbstractValidator<ListPatientProposalsRequest>
{
    private static readonly string[] AllowedStates = { "pending", "approved", "rejected", "invalidated" };

    public ListPatientProposalsRequestValidator()
    {
        RuleFor(x => x.ProposalState)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedStates.Contains(x.ToLower()))
            .WithMessage("proposal_state must be one of pending|approved|rejected|invalidated.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40);
    }
}
