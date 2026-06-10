using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;

/// <summary>
/// Handles searching drug problems for the current user.
/// </summary>
internal sealed class SearchDrugProblemsHandler(IDrugProblemRepository _drugProblemRepository)
    : IRequestHandler<SearchDrugProblemsQuery, ErrorOr<SearchDrugProblemsResponse>>
{
    public async Task<ErrorOr<SearchDrugProblemsResponse>> Handle(
        SearchDrugProblemsQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        List<DrugProblem> rows = await _drugProblemRepository.SearchByUserIdAsync(
            query.UserId,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = rows.Count > query.Take;
        List<DrugProblem> page = hasMore
            ? rows.Take(query.Take).ToList()
            : rows;

        IReadOnlyCollection<DrugProblemListItemDto> items = page
            .Select(drugProblem => drugProblem.MapToListItemDto())
            .ToList();

        if (query.Skip == 0)
        {
            totalCount = rows.Count == 0
                ? 0
                : await _drugProblemRepository.CountByUserIdAsync(query.UserId, cancellationToken);
        }

        return new SearchDrugProblemsResponse(items, totalCount, hasMore);
    }
}
