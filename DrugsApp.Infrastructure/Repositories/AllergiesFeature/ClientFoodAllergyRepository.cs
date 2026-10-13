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

public sealed class ClientFoodAllergyRepository(DrugsAppDbContext _context) : IClientFoodAllergyRepository
{
    public async Task<List<ClientFoodAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientFoodAllergy> query = _context.ClientFoodAllergies
            .AsNoTracking()
            .Include(x => x.Food)
            .Where(x => x.UserId == userId);

        query = ApplySort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientFoodAllergy>> ListAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientFoodAllergy> query = _context.ClientFoodAllergies
            .AsNoTracking()
            .Include(x => x.Food)
            .Where(x => x.UserId == userId);

        return await ApplySort(query).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientFoodAllergies
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ClientFoodAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ClientFoodAllergies
            .Include(x => x.Food)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ClientFoodAllergy allergy, CancellationToken cancellationToken)
    {
        await _context.ClientFoodAllergies.AddAsync(allergy, cancellationToken);
    }

    public void Remove(ClientFoodAllergy allergy)
    {
        _context.ClientFoodAllergies.Remove(allergy);
    }

    public async Task<bool> ExistsByFoodIdAsync(
        Guid userId,
        Guid foodId,
        CancellationToken cancellationToken)
    {
        return await _context.ClientFoodAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.FoodId == foodId,
                cancellationToken);
    }

    public async Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customFoodNameNormalized,
        CancellationToken cancellationToken)
    {
        return await _context.ClientFoodAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Custom &&
                     x.CustomFoodNameNormalized == customFoodNameNormalized,
                cancellationToken);
    }

    public async Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string foodDisplayNameNormalized,
        CancellationToken cancellationToken)
    {
        return await _context.ClientFoodAllergies
            .AsNoTracking()
            .Include(x => x.Food)
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.Food != null &&
                     x.Food.Name.ToLower() == foodDisplayNameNormalized,
                cancellationToken);
    }

    private static IQueryable<ClientFoodAllergy> ApplySort(IQueryable<ClientFoodAllergy> query)
    {
        return query
            .OrderBy(allergy =>
                allergy.Source == AllergyRecordSource.Database && allergy.Food != null
                    ? allergy.Food.Name
                    : allergy.CustomFoodName ?? string.Empty)
            .ThenBy(allergy => allergy.Id);
    }
}
