using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.SupplementFeature;

public sealed class SupplementRepository(DrugsAppDbContext _context) : ISupplementRepository
{
    private const double SimilarityThreshold = 0.35;

    private static IQueryable<Supplement> ApplySearch(IQueryable<Supplement> query, string? nameFilter)
    {
        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return query;
        }
        
        string trimmedQuery = nameFilter.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(supplement =>
            EF.Functions.ILike(supplement.SearchText, prefixPattern) ||
            EF.Functions.ILike(supplement.SearchText, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(supplement.SearchText, trimmedQuery) >= SimilarityThreshold);
        
    }

    private static IQueryable<Supplement> ApplySortAndOrder(IQueryable<Supplement> query) => query.OrderBy(supplement => supplement.Name).ThenBy(supplement => supplement.Id);

    public async Task<List<Supplement>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<Supplement> query = _context.Supplements.AsNoTracking();

        // apply search 
        query = ApplySearch(query, nameFilter);

        // apply sort and order 
        query = ApplySortAndOrder(query);

        // apply paging
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<Supplement> query = _context.Supplements.AsNoTracking();

        // apply search 
        query = ApplySearch(query, nameFilter);

        // apply counting
        return await query
            .CountAsync(cancellationToken);
    }

    public async Task<Supplement?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Supplements
            .FirstOrDefaultAsync(supplement => supplement.Id == id, cancellationToken);
    }

    public async Task<Supplement?> GetByNameAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return await _context.Supplements
            .AsNoTracking()
            .FirstOrDefaultAsync(
                supplement => supplement.Name == normalizedName,
                cancellationToken);
    }

    public async Task<List<Supplement>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new List<Supplement>();
        }

        return await _context.Supplements
            .AsNoTracking()
            .Where(supplement => ids.Contains(supplement.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Supplement>> GetByIdsForLinkingAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new List<Supplement>();
        }

        return await _context.Supplements
            .Where(supplement => ids.Contains(supplement.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Supplement supplement,
        CancellationToken cancellationToken)
    {
        await _context.Supplements.AddAsync(supplement, cancellationToken);
    }

    public void Remove(Supplement supplement)
    {
        _context.Supplements.Remove(supplement);
    }

    public async Task<bool> IsInAnyGroupAsync(
        Guid supplementId,
        CancellationToken cancellationToken)
    {
        return await _context.SupplementGroups
            .AsNoTracking()
            .AnyAsync(
                group => group.Supplements.Any(supplement => supplement.Id == supplementId),
                cancellationToken);
    }


}
