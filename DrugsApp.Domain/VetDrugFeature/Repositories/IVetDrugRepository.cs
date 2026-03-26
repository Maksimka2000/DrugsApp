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
}

