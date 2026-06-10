using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes;

/// <summary>
/// Handles searching drug problem types.
/// </summary>
internal sealed class SearchDrugProblemTypesHandler(IDrugProblemTypeRepository _drugProblemTypeRepository)
    : IRequestHandler<SearchDrugProblemTypesQuery, ErrorOr<SearchDrugProblemTypesResponse>>
{
    public async Task<ErrorOr<SearchDrugProblemTypesResponse>> Handle(
        SearchDrugProblemTypesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        List<DrugProblemType> rows = await _drugProblemTypeRepository.SearchAsync(
            query.ProblemTypeNameQuery,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = rows.Count > query.Take;
        List<DrugProblemType> page = hasMore
            ? rows.Take(query.Take).ToList()
            : rows;

        IReadOnlyCollection<DrugProblemTypeDto> items = page
            .Select(problemType => problemType.MapToDto())
            .ToList();

        if (query.Skip == 0)
        {
            totalCount = rows.Count == 0
                ? 0
                : await _drugProblemTypeRepository.CountAsync(query.ProblemTypeNameQuery, cancellationToken);
        }

        return new SearchDrugProblemTypesResponse(items, totalCount, hasMore);
    }
}
