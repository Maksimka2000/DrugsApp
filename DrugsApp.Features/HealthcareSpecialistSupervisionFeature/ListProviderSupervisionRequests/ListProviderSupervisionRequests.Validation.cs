using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderSupervisionRequests;

public sealed class ListProviderSupervisionRequestsRequestValidator
    : AbstractValidator<ListProviderSupervisionRequestsRequest>
{
    private static readonly string[] AllowedOrderBy = ["ascending", "descending"];

    public ListProviderSupervisionRequestsRequestValidator()
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
