using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.VetDrugFeature.Entities;

namespace DrugsApp.Domain.VetDrugFeature.Repositories;


public interface IVetDrugRepository
{

    Task<List<VetDrug>> SearchAsync(
        string? nameFilter,
        bool isAscending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<VetDrug?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<VetDrug?> GetByCommercialNameAsync(string commercialName, CancellationToken cancellationToken);

    Task AddAsync(VetDrug vetDrug, CancellationToken cancellationToken);

    /// <summary>
    /// Searches veterinary drugs by commercial name using prefix, contains, and trigram matching.
    /// </summary>
    /// <param name="searchQuery">Commercial name search query.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of veterinary drugs ordered by match relevance.</returns>
    Task<List<VetDrug>> SearchByCommercialNameAsync(
        string searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the total number of veterinary drugs matching the commercial name search query.
    /// </summary>
    /// <param name="searchQuery">Commercial name search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of matching veterinary drugs.</returns>
    Task<int> CountByCommercialNameAsync(
        string searchQuery,
        CancellationToken cancellationToken);
}

