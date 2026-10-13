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
        IQueryable<DrugProblem> query = ApplyCatalogIncludes(_context.DrugProblems.AsNoTracking())
            .Where(drugProblem => drugProblem.UserId == userId);

        query = query
            .OrderBy(drugProblem =>
                drugProblem.CustomDrugName != null
                    ? drugProblem.CustomDrugName.ToLower()
                    : drugProblem.DrugForm != null && drugProblem.DrugForm.Drug != null
                        ? drugProblem.DrugForm.Drug.CommercialName.ToLower()
                        : string.Empty)
            .ThenBy(drugProblem => drugProblem.Id);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DrugProblem>> ListAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await ApplyCatalogIncludes(_context.DrugProblems.AsNoTracking())
            .Where(drugProblem => drugProblem.UserId == userId)
            .OrderBy(drugProblem =>
                drugProblem.CustomDrugName != null
                    ? drugProblem.CustomDrugName.ToLower()
                    : drugProblem.DrugForm != null && drugProblem.DrugForm.Drug != null
                        ? drugProblem.DrugForm.Drug.CommercialName.ToLower()
                        : string.Empty)
            .ThenBy(drugProblem => drugProblem.Id)
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
        IQueryable<DrugProblem> query = ApplyCatalogIncludes(_context.DrugProblems)
            .Where(drugProblem => drugProblem.UserId == userId && drugProblem.Id == id);

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

    public async Task<bool> ExistsForUserWithDrugFormIdAsync(
        Guid userId,
        Guid drugFormId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblem> query = _context.DrugProblems
            .AsNoTracking()
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.DrugFormId == drugFormId);

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
        IQueryable<DrugProblem> query = ApplyCatalogIncludes(_context.DrugProblems.AsNoTracking())
            .Where(drugProblem =>
                drugProblem.UserId == userId &&
                drugProblem.DrugFormId != null &&
                drugProblem.DrugForm != null &&
                drugProblem.DrugForm.Drug != null &&
                EF.Functions.ILike(drugProblem.DrugForm.Drug.CommercialName, normalizedDrugDisplayName));

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

    private static IQueryable<DrugProblem> ApplyCatalogIncludes(IQueryable<DrugProblem> query) =>
        query
            .Include(drugProblem => drugProblem.DrugForm)
                .ThenInclude(drugForm => drugForm!.Drug)
                    .ThenInclude(drug => drug.ActiveSubstances)
            .Include(drugProblem => drugProblem.DrugForm)
                .ThenInclude(drugForm => drugForm!.PharmaceuticalForms);
}
