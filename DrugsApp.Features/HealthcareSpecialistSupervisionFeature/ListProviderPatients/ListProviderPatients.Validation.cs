using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

public sealed class ListProviderPatientsRequestValidator : AbstractValidator<ListProviderPatientsRequest>
{
    private static readonly string[] AllowedRelationshipStates = { "all", "pending", "active" };
    private static readonly string[] AllowedSortBy = { "status", "date_added", "patient_name" };
    private static readonly string[] AllowedSortDirection = { "asc", "desc" };

    public ListProviderPatientsRequestValidator()
    {
        RuleFor(x => x.RelationshipState)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedRelationshipStates.Contains(x.ToLower()))
            .WithMessage("relationship_state must be one of all|pending|active.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x.ToLower()))
            .WithMessage("sort_by must be one of status|date_added|patient_name.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortDirection.Contains(x.ToLower()))
            .WithMessage("sort_direction must be one of asc|desc.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40);
    }
}
