using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.ListOtherAllergies;

internal sealed class ListOtherAllergiesHandler(IClientOtherAllergyRepository _repository)
    : IRequestHandler<ListOtherAllergiesQuery, ErrorOr<ListOtherAllergiesResponse>>
{
    public async Task<ErrorOr<ListOtherAllergiesResponse>> Handle(
        ListOtherAllergiesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        List<ClientOtherAllergy> rows = await _repository.SearchAsync(query.UserId, query.Skip, takePlusOne, cancellationToken);
        bool hasMore = rows.Count > query.Take;
        List<ClientOtherAllergyDto> page = rows.Take(query.Take).MapToOtherDtoList();
        int totalCount = 0;
        if (query.Skip == 0)
            totalCount = await _repository.CountAsync(query.UserId, cancellationToken);
        return page.MapToResponse(totalCount, hasMore);
    }
}
