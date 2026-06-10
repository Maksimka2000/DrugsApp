using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AllergiesFeature;

public sealed class ClientOtherAllergyRepository(DrugsAppDbContext _context) : IClientOtherAllergyRepository
{
    public async Task<List<ClientOtherAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await _context.ClientOtherAllergies
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.AllergyName)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientOtherAllergies
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ClientOtherAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ClientOtherAllergies
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ClientOtherAllergy allergy, CancellationToken cancellationToken)
    {
        await _context.ClientOtherAllergies.AddAsync(allergy, cancellationToken);
    }

    public void Remove(ClientOtherAllergy allergy)
    {
        _context.ClientOtherAllergies.Remove(allergy);
    }

    public async Task<bool> ExistsNameNormalizedAsync(
        Guid userId,
        string allergyNameNormalized,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientOtherAllergy> query = _context.ClientOtherAllergies
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.AllergyNameNormalized == allergyNameNormalized);

        if (excludeId is Guid excludedId)
        {
            query = query.Where(x => x.Id != excludedId);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
