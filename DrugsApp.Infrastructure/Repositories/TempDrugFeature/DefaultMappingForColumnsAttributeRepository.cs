using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.TempDrugFeature;

public sealed class DefaultMappingForColumnsAttributeRepository(DrugsAppDbContext context)
    : IDefaultMappingForColumnsAttributeRepository
{
    private readonly DrugsAppDbContext _context = context;

    public async Task<List<DefaultMappingForColumnsAttribute>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.DefaultMappingForColumnsAttributes
            .AsNoTracking()
            .OrderBy(mapping => mapping.SourceColumnName)
            .ToListAsync(cancellationToken);
    }
}

