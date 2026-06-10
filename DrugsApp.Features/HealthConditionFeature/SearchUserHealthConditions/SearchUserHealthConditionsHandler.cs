using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions;

/// <summary>
/// Handles search queries for user health conditions.
/// </summary>
public sealed class SearchUserHealthConditionsHandler(
    IUserHealthConditionRepository _userHealthConditionRepository)
    : IRequestHandler<SearchUserHealthConditionsQuery, ErrorOr<SearchUserHealthConditionsResponse>>
{
    public async Task<ErrorOr<SearchUserHealthConditionsResponse>> Handle(
        SearchUserHealthConditionsQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;

        List<UserHealthCondition> rows = await _userHealthConditionRepository.SearchAsync(
            query.UserId,
            query.DisplayNameQuery,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = rows.Count > query.Take;
        List<UserHealthConditionDto> page = rows
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _userHealthConditionRepository.CountAsync(
                query.UserId,
                query.DisplayNameQuery,
                cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
