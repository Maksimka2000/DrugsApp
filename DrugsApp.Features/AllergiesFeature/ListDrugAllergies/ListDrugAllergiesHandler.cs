using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.ListDrugAllergies;

/// <summary>
/// Handles listing drug allergies for the current user.
/// </summary>
internal sealed class ListDrugAllergiesHandler(IClientDrugAllergyRepository _drugAllergyRepository)
    : IRequestHandler<ListDrugAllergiesQuery, ErrorOr<ListDrugAllergiesResponse>>
{
    public async Task<ErrorOr<ListDrugAllergiesResponse>> Handle(
        ListDrugAllergiesQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;

        List<ClientDrugAllergy> rows = await _drugAllergyRepository.SearchAsync(
            query.UserId,
            query.Skip,
            takePlusOne,
            cancellationToken);

        bool hasMore = rows.Count > query.Take;
        List<ClientDrugAllergyDto> page = rows
            .Take(query.Take)
            .MapToDrugDtoList();

        int totalCount = 0;
        if (query.Skip == 0)
        {
            totalCount = await _drugAllergyRepository.CountAsync(query.UserId, cancellationToken);
        }

        return page.MapToResponse(totalCount, hasMore);
    }
}
