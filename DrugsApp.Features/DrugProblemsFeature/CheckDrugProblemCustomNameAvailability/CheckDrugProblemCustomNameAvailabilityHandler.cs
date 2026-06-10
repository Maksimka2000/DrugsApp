using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;

/// <summary>
/// Handles custom drug problem name availability checks.
/// </summary>
internal sealed class CheckDrugProblemCustomNameAvailabilityHandler(IDrugProblemRepository _drugProblemRepository)
    : IRequestHandler<CheckDrugProblemCustomNameAvailabilityQuery, ErrorOr<DrugProblemCustomNameAvailabilityDto>>
{
    public async Task<ErrorOr<DrugProblemCustomNameAvailabilityDto>> Handle(
        CheckDrugProblemCustomNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = NormalizeForLookup(query.CustomDrugName);

        Guid? conflictingId = await _drugProblemRepository.FindConflictingCustomNameAsync(
            query.UserId,
            normalizedName,
            query.ExcludeId,
            cancellationToken);

        if (conflictingId is not null)
        {
            return new DrugProblemCustomNameAvailabilityDto(false, conflictingId);
        }

        bool hasCatalogCollision = await _drugProblemRepository.ExistsForUserWithAnyDrugDisplayNameAsync(
            query.UserId,
            normalizedDrugDisplayName: normalizedName,
            excludeId: null,
            cancellationToken);

        if (hasCatalogCollision)
        {
            return new DrugProblemCustomNameAvailabilityDto(false, null);
        }

        return new DrugProblemCustomNameAvailabilityDto(true, null);
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
