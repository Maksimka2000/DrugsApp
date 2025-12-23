using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Features.PharmaceuticalFormFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PharmaceuticalFormFeature.SearchPharmaceuticalForms;

/// <summary>
/// Handles searching pharmaceutical forms by formulation name with pagination.
/// </summary>
internal sealed class SearchPharmaceuticalFormsHandler
    : IRequestHandler<SearchPharmaceuticalFormsQuery, ErrorOr<SearchPharmaceuticalFormsResponse>>
{
    private readonly IPharmaceuticalFormRepository _pharmaceuticalFormRepository;

    public SearchPharmaceuticalFormsHandler(IPharmaceuticalFormRepository pharmaceuticalFormRepository)
    {
        _pharmaceuticalFormRepository = pharmaceuticalFormRepository;
    }

    /// <summary>
    /// Searches pharmaceutical forms by name with pagination.
    /// </summary>
    /// <param name="query">Search parameters including name filter and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing items and paging metadata.</returns>
    public async Task<ErrorOr<SearchPharmaceuticalFormsResponse>> Handle(
        SearchPharmaceuticalFormsQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedNameFilter = string.IsNullOrWhiteSpace(query.Name)
            ? null
            : query.Name.Trim();

        List<PharmaceuticalForm> pagePlusOne = await _pharmaceuticalFormRepository.SearchAsync(
            normalizedNameFilter,
            query.Skip,
            query.Take + 1,
            cancellationToken);

        bool hasMore = pagePlusOne.Count > query.Take;

        List<PharmaceuticalFormDto> items = pagePlusOne
            .Take(query.Take)
            .MapToDtoList();

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await _pharmaceuticalFormRepository.CountAsync(
                normalizedNameFilter,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }
}


