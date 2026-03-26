using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugFeature.SearchDrugSimilarities;


/// <summary>
/// Handles searching drug similarities.
/// </summary>
/// <param name="_drugRepository">Drug repository.</param>
/// <param name="_drugSimilarityDomainService">Drug similarity domain service.</param>
internal sealed class SearchDrugSimilaritiesHandler(
    IDrugRepository _drugRepository,
    IDrugSimilarityDomainService _drugSimilarityDomainService)
        : IRequestHandler<SearchDrugSimilaritiesQuery, ErrorOr<SearchDrugSimilaritiesResponse>>
{
    
    /// <summary>
    /// Searches drug similarities.
    /// </summary>
    /// <param name="query">Search drug similarities query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search drug similarities response.</returns>
    public async Task<ErrorOr<SearchDrugSimilaritiesResponse>> Handle(
        SearchDrugSimilaritiesQuery query,
        CancellationToken cancellationToken)
    {
        List<Drug> candidates = await _drugRepository.SearchSimilarCandidatesAsync(
            commercialName: query.Request.CommercialName,
            typeOfProduct: query.Request.TypeOfProduct,
            cancellationToken: cancellationToken);

        DrugSimilaritySnapshot snapshot = query.Request.MapToSnapshot();

        var results = candidates
            .Select(candidate => new { Candidate = candidate, Result = _drugSimilarityDomainService.ScoreAndClassify(snapshot, candidate) })
            .Where(x => !x.Result.IsError)
            .Select(x => x.Result.Value.MapToDto(x.Candidate))
            .OrderByDescending(x => x.MatchScorePercent)
            .ToList();

        return new SearchDrugSimilaritiesResponse(Items: results);
    }
}

