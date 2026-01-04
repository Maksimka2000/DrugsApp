using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.RetrieveDrug;

/// <summary>
/// Handles retrieving a drug by id.
/// </summary>
/// <param name="_drugRepository">Drug repository.</param>
internal sealed class RetrieveDrugHandler(IDrugRepository _drugRepository) : IRequestHandler<RetrieveDrugQuery, ErrorOr<DrugDto>>
{
    /// <summary>
    /// Retrieves a drug by id.
    /// </summary>
    /// <param name="query">Retrieve drug query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested drug.</returns>
    public async Task<ErrorOr<DrugDto>> Handle(
        RetrieveDrugQuery query,
        CancellationToken cancellationToken)
    {
        Drug? drug = await _drugRepository.GetByIdAsync(query.DrugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "drug",
                    ["resourceId"] = query.DrugId.ToString()
                });
        }

        return drug.MapToDto();
    }
}

