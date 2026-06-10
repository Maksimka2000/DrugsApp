using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.DrugProblemsFeature;

public sealed class DrugProblemRepository : IDrugProblemRepository
{
    private readonly DrugsAppDbContext _context;

    public DrugProblemRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DrugProblem>> SearchByUserIdAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem => drugProblem.UserId == userId)
            .Include(drugProblem => drugProblem.Drug)
                .ThenInclude(drug => drug!.ActiveSubstances);

        query = query
            .OrderBy(drugProblem =>
                drugProblem.CustomDrugName != null
                    ? drugProblem.CustomDrugName.ToLower()
                    : drugProblem.Drug != null
                        ? drugProblem.Drug.CommercialName.ToLower()
                        : string.Empty)
            .ThenBy(drugProblem => drugProblem.Id);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem => drugProblem.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<DrugProblem?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .Where(drugProblem => drugProblem.UserId == userId && drugProblem.Id == id)
            .Include(drugProblem => drugProblem.Drug)
                .ThenInclude(drug => drug!.ActiveSubstances);

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(DrugProblem drugProblem, CancellationToken cancellationToken)
    {
        await _context.DrugProblems.AddAsync(drugProblem, cancellationToken);
    }

    public void Remove(DrugProblem drugProblem)
    {
        _context.DrugProblems.Remove(drugProblem);
    }

    public async Task<bool> ExistsForUserWithDrugIdAsync(
        Guid userId,
        Guid drugId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.DrugId == drugId);

        if (excludeId.HasValue)
        {
            query = query.Where(drugProblem => drugProblem.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsForUserWithCustomNameAsync(
        Guid userId,
        string normalizedCustomName,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.CustomDrugName != null &&
                EF.Functions.ILike(drugProblem.CustomDrugName, normalizedCustomName));

        if (excludeId.HasValue)
        {
            query = query.Where(drugProblem => drugProblem.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsForUserWithAnyDrugDisplayNameAsync(
        Guid userId,
        string normalizedDrugDisplayName,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.DrugId != null &&
                drugProblem.Drug != null &&
                EF.Functions.ILike(drugProblem.Drug.CommercialName, normalizedDrugDisplayName));

        if (excludeId.HasValue)
        {
            query = query.Where(drugProblem => drugProblem.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Guid?> FindConflictingCustomNameAsync(
        Guid userId,
        string normalizedCustomName,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.CustomDrugName != null &&
                EF.Functions.ILike(drugProblem.CustomDrugName, normalizedCustomName));

        if (excludeId.HasValue)
        {
            query = query.Where(drugProblem => drugProblem.Id != excludeId.Value);
        }

        return await query
            .Select(drugProblem => (Guid?)drugProblem.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
