using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.DrugFeature.Repositories;

/// <summary>
/// Persistence contract dedicated to drug form lookups for update workflows.
/// </summary>
public interface IDrugFormRepository
{
    /// <summary>
    /// Loads a drug form with all required navigation properties for update scenarios.
    /// </summary>
    /// <param name="drugFormId">Drug form identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked drug form aggregate or null.</returns>
    Task<DrugForm?> GetByIdAsync(Guid drugFormId, CancellationToken cancellationToken);
}

