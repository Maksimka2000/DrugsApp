using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.VetDrugFeature.ApplicationServices;


/// <summary>
/// DTO special for the vet drug canonicalization service for creating a veterinary drug from a review.
/// </summary>
/// <param name="CommercialName"></param>
/// <param name="TypeOfProduct"></param>
/// <param name="ProductIdentifier"></param>
public sealed record CreateVetDrugFromReviewVetDrugApplicationServiceDraft(
    string CommercialName,
    string TypeOfProduct,
    string ProductIdentifier);



/// <summary>
/// Service for canonicalizing veterinary drugs from a review.
/// </summary>
/// <param name="_vetDrugRepository"></param>
internal sealed class VetDrugCanonicalizationService(IVetDrugRepository _vetDrugRepository) : IVetDrugCanonicalizationService
{

    /// <summary>
    /// Creates a veterinary drug from a review.
    /// </summary>
    /// <param name="vetDrugApplicationServiceDraft">The draft of the veterinary drug application service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The veterinary drug.</returns>
    public async Task<ErrorOr<VetDrugDto>> CreateVetDrugFromReviewAsync(
        CreateVetDrugFromReviewVetDrugApplicationServiceDraft vetDrugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Validate the product identifier
        if (!int.TryParse(vetDrugApplicationServiceDraft.ProductIdentifier, out int productIdentifier) || productIdentifier <= 0)
        {
            return Error.Validation(
                description: "ProductIdentifier must be a positive integer.",
                metadata: new()
                {
                    ["error"] = "productIdentifier"
                });
        }

        // Get the existing veterinary drug by commercial name
        VetDrug? existing = await _vetDrugRepository.GetByCommercialNameAsync(vetDrugApplicationServiceDraft.CommercialName, cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict(
                description: "A veterinary drug with the same commercial name already exists.",
                metadata: new()
                {
                    ["existingResourceId"] = existing.Id.ToString()
                });
        }

        // Create the veterinary drug
        var vetDrug = VetDrug.Create(
            productIdentifier: productIdentifier,
            commercialName: vetDrugApplicationServiceDraft.CommercialName,
            typeOfProduct: vetDrugApplicationServiceDraft.TypeOfProduct);

        // Add the veterinary drug to the repository
        await _vetDrugRepository.AddAsync(vetDrug, cancellationToken);

        // Map the veterinary drug to a DTO (Data Transfer Object its prohibited to return the entity)
        return vetDrug.MapToDto();
    }
}

