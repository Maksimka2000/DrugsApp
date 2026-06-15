using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

public sealed class SearchPatientHealthcareProfessionalsRequestValidator
    : AbstractValidator<SearchPatientHealthcareProfessionalsRequest>
{
    private static readonly string[] AllowedSortBy = PatientHealthcareProfessionalSearchCatalog.GetSortFieldTokens().ToArray();
    private static readonly string[] AllowedOrderBy = { "ascending", "descending" };

    public SearchPatientHealthcareProfessionalsRequestValidator()
    {
        RuleFor(request => request.SortBy)
            .NotEmpty()
            .Must(sortBy => AllowedSortBy.Contains(sortBy.ToLowerInvariant()))
            .WithMessage("sort_by must be one of: status, date_added, display_name.");

        RuleFor(request => request.OrderBy)
            .NotEmpty()
            .Must(orderBy => AllowedOrderBy.Contains(orderBy.ToLowerInvariant()))
            .WithMessage("order_by must be one of: ascending, descending.");

        When(request => request.Statuses is not null, () =>
        {
            RuleFor(request => request.Statuses!)
                .Must(statuses => statuses.All(status =>
                    PatientHealthcareProfessionalSearchCatalog.TryParseRelationshipStatus(
                        status,
                        out _)))
                .WithMessage("statuses must be one of: pending, active, rejected, cancelled, expired, removed.")
                .Must(statuses => statuses
                    .Select(status => status.Trim().ToLowerInvariant())
                    .Distinct()
                    .Count() == statuses.Count)
                .WithMessage("Duplicate statuses values are not allowed.");
        });

        RuleFor(request => request.SearchQuery)
            .MaximumLength(256);

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
