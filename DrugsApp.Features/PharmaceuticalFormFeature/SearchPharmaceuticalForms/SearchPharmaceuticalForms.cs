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
        // initialize normalized name filter
        string? normalizedFormulationName = NormalizeFormulationName(query.FormulationName);
         // nomalize filter
        // determine sort order
        // get sort field
        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // search pharmaceutical forms
        List<PharmaceuticalForm> pharmaceuticalForms = await _pharmaceuticalFormRepository.SearchAsync(
            normalizedFormulationName,
            query.Skip,
            takePlusOne,
            cancellationToken);

        // determine if there are more items
        bool hasMore = pharmaceuticalForms.Count > query.Take;

        // get page
        List<PharmaceuticalForm> page = hasMore
            ? pharmaceuticalForms.Take(query.Take).ToList()
            : pharmaceuticalForms;

        // map to DTOs
        List<PharmaceuticalFormDto> items = page.MapToDtoList();

        // count total items
        if (query.Skip == 0)
        {
            totalCount = await _pharmaceuticalFormRepository.CountAsync(
                normalizedFormulationName,
                cancellationToken);
        }

        return items.MapToResponse(totalCount, hasMore);
    }

    private static string? NormalizeFormulationName(string? formulationName)
    {
        if (string.IsNullOrWhiteSpace(formulationName))
        {
            return null;
        }

        return formulationName.Trim();
    }
}


