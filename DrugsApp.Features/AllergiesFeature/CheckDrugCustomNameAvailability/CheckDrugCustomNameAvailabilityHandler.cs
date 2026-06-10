using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CheckDrugCustomNameAvailability;

/// <summary>
/// Handles drug custom name availability checks.
/// </summary>
internal sealed class CheckDrugCustomNameAvailabilityHandler(
    IClientDrugAllergyRepository _drugAllergyRepository)
    : IRequestHandler<CheckDrugCustomNameAvailabilityQuery, ErrorOr<NameAvailabilityResponse>>
{
    public async Task<ErrorOr<NameAvailabilityResponse>> Handle(
        CheckDrugCustomNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = AllergySharedMappings.NormalizeForLookup(query.CustomDrugName);

        bool existsCustom = await _drugAllergyRepository.ExistsCustomNameNormalizedAsync(
            query.UserId,
            normalizedName,
            cancellationToken);

        if (existsCustom)
        {
            return new NameAvailabilityResponse(false);
        }

        bool existsDatabase = await _drugAllergyRepository.ExistsDatabaseDisplayNameNormalizedAsync(
            query.UserId,
            normalizedName,
            cancellationToken);

        return new NameAvailabilityResponse(!existsDatabase);
    }
}
