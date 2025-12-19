using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using ErrorOr;
using MediatR;
using DrugsApp.Domain.VetDrugFeature.Entities;

namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;



/// <summary>
/// Handles updating a veterinary drug's commercial name.
/// </summary>
internal sealed class UpdateVetDrugHandler
    : IRequestHandler<UpdateVetDrugCommand, ErrorOr<UpdatedVetDrugResponse>>
{
    private readonly IVetDrugRepository _vetDrugRepository;

    public UpdateVetDrugHandler(IVetDrugRepository vetDrugRepository)
    {
        _vetDrugRepository = vetDrugRepository;
    }

    /// <summary>
    /// Updates the veterinary drug's commercial name.
    /// </summary>
    /// <param name="command">Update command containing the drug ID and new commercial name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated veterinary drug response with new commercial name.</returns>
    public async Task<ErrorOr<UpdatedVetDrugResponse>> Handle(
        UpdateVetDrugCommand command,
        CancellationToken cancellationToken)
    {
        VetDrug? vetDrug = await _vetDrugRepository.GetByIdAsync(
            command.VetDrugId,
            cancellationToken);

        if (vetDrug is null)
        {
            return Error.NotFound(
                description: "The requested veterinary drug was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["ResourceType"] = "VetDrug",
                    ["ResourceId"] = command.VetDrugId.ToString()
                });
        }

        
        if (command.Request.CommercialName is not null && command.Request.CommercialName != vetDrug.CommercialName)
        {
            vetDrug.UpdateCommercialName(command.Request.CommercialName);
        }

        return vetDrug.MapToUpdatedResponse();
    }
}

