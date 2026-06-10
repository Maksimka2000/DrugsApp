using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.ListFoodAllergies;

internal sealed class ListFoodAllergiesHandler(IClientFoodAllergyRepository _repository)
    : IRequestHandler<ListFoodAllergiesQuery, ErrorOr<ListFoodAllergiesResponse>>
{
    public async Task<ErrorOr<ListFoodAllergiesResponse>> Handle(
        ListFoodAllergiesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        List<ClientFoodAllergy> rows = await _repository.SearchAsync(
            query.UserId, query.Skip, takePlusOne, cancellationToken);
        bool hasMore = rows.Count > query.Take;
        List<ClientFoodAllergyDto> page = rows.Take(query.Take).MapToFoodDtoList();
        int totalCount = 0;
        if (query.Skip == 0)
            totalCount = await _repository.CountAsync(query.UserId, cancellationToken);
        return page.MapToResponse(totalCount, hasMore);
    }
}
