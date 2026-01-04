using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using MediatR;


namespace DrugsApp.Features.VetDrugFeature.RetrieveVetDrug;

/// <summary>
/// Handles retrieving a veterinary drug by id.
/// </summary>
/// <param name="_vetDrugRepository">The veterinary drug repository.</param>
internal sealed class RetrieveVetDrugHandler(IVetDrugRepository _vetDrugRepository) : IRequestHandler<RetrieveVetDrugQuery, ErrorOr<VetDrugDto>>
{
    /// <summary>
    /// Retrieves a veterinary drug by id.
    /// </summary>
    /// <param name="query">The query to retrieve a veterinary drug by id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The veterinary drug.</returns>
    public async Task<ErrorOr<VetDrugDto>> Handle(
        RetrieveVetDrugQuery query,
        CancellationToken cancellationToken)
    {
        VetDrug? vetDrug = await _vetDrugRepository.GetByIdAsync(query.VetDrugId, cancellationToken);
        if (vetDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "vetDrug",
                    ["resourceId"] = query.VetDrugId.ToString()
                });
        }

        return vetDrug.MapToDto();
    }
}

