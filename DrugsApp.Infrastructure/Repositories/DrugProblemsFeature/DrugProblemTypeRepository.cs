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

public sealed class DrugProblemTypeRepository : IDrugProblemTypeRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public DrugProblemTypeRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DrugProblemType>> SearchAsync(
        string? problemTypeNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblemType> query = _context.DrugProblemTypes.AsNoTracking();

        query = ApplyNameFilter(query, problemTypeNameQuery);
        query = ApplyOrdering(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? problemTypeNameQuery, CancellationToken cancellationToken)
    {
        IQueryable<DrugProblemType> query = _context.DrugProblemTypes.AsNoTracking();

        query = ApplyNameFilter(query, problemTypeNameQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<DrugProblemType?> GetByIdAsync(
        Guid id,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugProblemType> query = _context.DrugProblemTypes
            .Where(drugProblemType => drugProblemType.Id == id);

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DrugProblemType?> GetByNameAsync(
        string normalizedProblemTypeName,
        CancellationToken cancellationToken)
    {
        return await _context.DrugProblemTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                drugProblemType => EF.Functions.ILike(
                    drugProblemType.ProblemTypeName,
                    normalizedProblemTypeName),
                cancellationToken);
    }

    public async Task AddAsync(DrugProblemType drugProblemType, CancellationToken cancellationToken)
    {
        await _context.DrugProblemTypes.AddAsync(drugProblemType, cancellationToken);
    }

    public void Remove(DrugProblemType drugProblemType)
    {
        _context.DrugProblemTypes.Remove(drugProblemType);
    }

    private static IQueryable<DrugProblemType> ApplyNameFilter(
        IQueryable<DrugProblemType> query,
        string? problemTypeNameQuery)
    {
        if (string.IsNullOrWhiteSpace(problemTypeNameQuery))
        {
            return query;
        }

        string trimmed = problemTypeNameQuery.Trim();
        string prefixPattern = $"{trimmed}%";
        string containsPattern = $"%{trimmed}%";
        bool useTrigram = trimmed.Length >= 3;

        return query.Where(problemType =>
            EF.Functions.ILike(problemType.ProblemTypeName, prefixPattern) ||
            EF.Functions.ILike(problemType.ProblemTypeName, containsPattern) ||
            useTrigram && EF.Functions.TrigramsSimilarity(problemType.ProblemTypeName, trimmed) >= SimilarityThreshold);
    }

    private static IQueryable<DrugProblemType> ApplyOrdering(IQueryable<DrugProblemType> query) =>
        query
            .OrderBy(problemType => problemType.ProblemTypeName.ToLower())
            .ThenBy(problemType => problemType.Id);
}
