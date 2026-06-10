using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.SearchHealthConditions;

/// <summary>
/// Handles search queries for catalog health conditions.
/// </summary>
public sealed class SearchHealthConditionsHandler(
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<SearchHealthConditionsQuery, ErrorOr<SearchHealthConditionsResponse>>
{
    public async Task<ErrorOr<SearchHealthConditionsResponse>> Handle(
        SearchHealthConditionsQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;

        List<HealthCondition> rows = await _healthConditionRepository.SearchAsync(
            query.HealthConditionNameQuery,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = rows.Count > query.Take;
        List<HealthConditionDto> page = rows
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _healthConditionRepository.CountAsync(
                query.HealthConditionNameQuery,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
