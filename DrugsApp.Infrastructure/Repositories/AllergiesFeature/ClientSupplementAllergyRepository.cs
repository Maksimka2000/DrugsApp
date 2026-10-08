using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AllergiesFeature;

public sealed class ClientSupplementAllergyRepository(DrugsAppDbContext _context) : IClientSupplementAllergyRepository
{
    public async Task<List<ClientSupplementAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientSupplementAllergy> query = _context.ClientSupplementAllergies
            .AsNoTracking()
            .Include(x => x.Supplement)
            .Where(x => x.UserId == userId);

        query = ApplySort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientSupplementAllergy>> ListAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientSupplementAllergy> query = _context.ClientSupplementAllergies
            .AsNoTracking()
            .Include(x => x.Supplement)
            .Where(x => x.UserId == userId);

        return await ApplySort(query).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientSupplementAllergies
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ClientSupplementAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ClientSupplementAllergies
            .Include(x => x.Supplement)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ClientSupplementAllergy allergy, CancellationToken cancellationToken)
    {
        await _context.ClientSupplementAllergies.AddAsync(allergy, cancellationToken);
    }

    public void Remove(ClientSupplementAllergy allergy)
    {
        _context.ClientSupplementAllergies.Remove(allergy);
    }

    public async Task<bool> ExistsBySupplementIdAsync(
        Guid userId,
        Guid supplementId,
        CancellationToken cancellationToken)
    {
        return await _context.ClientSupplementAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.SupplementId == supplementId,
                cancellationToken);
    }

    public async Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customSupplementNameNormalized,
        CancellationToken cancellationToken)
    {
        return await _context.ClientSupplementAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Custom &&
                     x.CustomSupplementNameNormalized == customSupplementNameNormalized,
                cancellationToken);
    }

    public async Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string supplementDisplayNameNormalized,
        CancellationToken cancellationToken)
    {
        return await _context.ClientSupplementAllergies
            .AsNoTracking()
            .Include(x => x.Supplement)
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.Supplement != null &&
                     x.Supplement.Name.ToLower() == supplementDisplayNameNormalized,
                cancellationToken);
    }

    private static IQueryable<ClientSupplementAllergy> ApplySort(IQueryable<ClientSupplementAllergy> query)
    {
        return query
            .OrderBy(allergy =>
                allergy.Source == AllergyRecordSource.Database && allergy.Supplement != null
                    ? allergy.Supplement.Name
                    : allergy.CustomSupplementName ?? string.Empty)
            .ThenBy(allergy => allergy.Id);
    }
}
