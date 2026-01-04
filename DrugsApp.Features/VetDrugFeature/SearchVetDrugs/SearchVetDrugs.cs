using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Features.VetDrugFeature.Shared;
using DrugsApp.Domain.VetDrugFeature.Entities;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;


/// <summary>
/// Handles searching veterinary drugs by commercial name with sorting and pagination.
/// </summary>
internal sealed class SearchVetDrugsHandler(IVetDrugRepository _vetDrugRepository)
        : IRequestHandler<SearchVetDrugsQuery, ErrorOr<List<VetDrugDto>>>
{

    /// <summary>
    /// Searches veterinary drugs by name with sorting and pagination.
    /// </summary>
    /// <param name="query">Search parameters including name filter, sort order, and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching veterinary drug DTOs.</returns>
    public async Task<ErrorOr<List<VetDrugDto>>> Handle(
        SearchVetDrugsQuery query,
        CancellationToken cancellationToken)
    {
        bool isAscending = query.Order.Equals("ascending", System.StringComparison.OrdinalIgnoreCase);

        List<VetDrug> vetDrugs = await _vetDrugRepository.SearchAsync(
            query.Name,
            isAscending,
            query.Skip,
            query.Take,
            cancellationToken);

        // Return empty list if no drugs found.
        if (vetDrugs.Count == 0)
        {
            return new List<VetDrugDto>();
        }

        return vetDrugs.MapToDtoList();
    }
}

