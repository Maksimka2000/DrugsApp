using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Domain.TempDrugFeature.Repositories;

/// <summary>
/// Read contract for default column mapping entries.
/// </summary>
public interface IDefaultMappingForColumnsAttributeRepository
{
    /// <summary>
    /// Returns all configured default mapping rows.
    /// </summary>
    Task<List<DefaultMappingForColumnsAttribute>> GetAllAsync(CancellationToken cancellationToken);
}

