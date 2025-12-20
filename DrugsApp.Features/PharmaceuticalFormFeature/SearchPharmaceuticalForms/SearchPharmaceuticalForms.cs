using System.Collections.Generic;
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
    : IRequestHandler<SearchPharmaceuticalFormsQuery, ErrorOr<List<PharmaceuticalFormDto>>>
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
    /// <returns>List of matching pharmaceutical form DTOs.</returns>
    public async Task<ErrorOr<List<PharmaceuticalFormDto>>> Handle(
        SearchPharmaceuticalFormsQuery query,
        CancellationToken cancellationToken)
    {
        List<PharmaceuticalForm> pharmaceuticalForms = await _pharmaceuticalFormRepository.SearchAsync(
            query.Name,
            query.Skip,
            query.Take,
            cancellationToken);

        if (pharmaceuticalForms.Count == 0)
        {
            return new List<PharmaceuticalFormDto>();
        }

        return pharmaceuticalForms.MapToDtoList();
    }
}


