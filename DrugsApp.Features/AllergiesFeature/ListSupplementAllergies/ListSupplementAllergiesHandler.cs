using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.ListSupplementAllergies;

internal sealed class ListSupplementAllergiesHandler(IClientSupplementAllergyRepository _repository)
    : IRequestHandler<ListSupplementAllergiesQuery, ErrorOr<ListSupplementAllergiesResponse>>
{
    public async Task<ErrorOr<ListSupplementAllergiesResponse>> Handle(
        ListSupplementAllergiesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        List<ClientSupplementAllergy> rows = await _repository.SearchAsync(
            query.UserId, query.Skip, takePlusOne, cancellationToken);
        bool hasMore = rows.Count > query.Take;
        List<ClientSupplementAllergyDto> page = rows.Take(query.Take).MapToSupplementDtoList();
        int totalCount = 0;
        if (query.Skip == 0)
            totalCount = await _repository.CountAsync(query.UserId, cancellationToken);
        return page.MapToResponse(totalCount, hasMore);
    }
}
