using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability;

/// <summary>
/// Handles drug problem type name availability checks.
/// </summary>
internal sealed class CheckDrugProblemTypeNameAvailabilityHandler(IDrugProblemTypeRepository _repository)
    : IRequestHandler<CheckDrugProblemTypeNameAvailabilityQuery, ErrorOr<DrugProblemTypeNameAvailabilityDto>>
{
    public async Task<ErrorOr<DrugProblemTypeNameAvailabilityDto>> Handle(
        CheckDrugProblemTypeNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = NormalizeForLookup(query.ProblemTypeName);

        DrugProblemType? existing = await _repository.GetByNameAsync(normalizedName, cancellationToken);
        if (existing is null || existing.Id == query.ExcludeId)
        {
            return new DrugProblemTypeNameAvailabilityDto(true, null);
        }

        return new DrugProblemTypeNameAvailabilityDto(false, existing.Id);
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
