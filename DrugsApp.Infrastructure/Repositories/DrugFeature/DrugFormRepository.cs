using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.DrugFeature;

public sealed class DrugFormRepository : IDrugFormRepository
{
    private readonly DrugsAppDbContext _context;

    public DrugFormRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<DrugForm?> GetByIdAsync(Guid drugFormId, CancellationToken cancellationToken)
    {
        return await _context.DrugForms
            .Include(df => df.RoutesOfAdministration)
            .Include(df => df.PharmaceuticalForms)
            .FirstOrDefaultAsync(df => df.Id == drugFormId, cancellationToken);
    }
}