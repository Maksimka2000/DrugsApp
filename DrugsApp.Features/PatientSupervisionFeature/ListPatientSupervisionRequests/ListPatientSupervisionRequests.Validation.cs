using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;

public sealed class ListPatientSupervisionRequestsRequestValidator
    : AbstractValidator<ListPatientSupervisionRequestsRequest>
{
    private static readonly string[] AllowedOrderBy = { "ascending", "descending" };

    public ListPatientSupervisionRequestsRequestValidator()
    {
        RuleFor(request => request.OrderBy)
            .NotEmpty()
            .Must(orderBy => AllowedOrderBy.Contains(orderBy.ToLowerInvariant()))
            .WithMessage("order_by must be one of: ascending, descending.");

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
