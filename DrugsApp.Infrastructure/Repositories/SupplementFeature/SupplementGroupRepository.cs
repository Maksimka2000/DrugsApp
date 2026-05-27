using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace DrugsApp.Infrastructure.Repositories.SupplementFeature;

public sealed class SupplementGroupRepository(DrugsAppDbContext context) : ISupplementGroupRepository
{
    private const double SimilarityThreshold = 0.35;

    private static IQueryable<SupplementGroup> ApplyIncludes(IQueryable<SupplementGroup> query) =>
        query.Include(group => group.Supplements);


    private static IQueryable<SupplementGroup> ApplySearch(IQueryable<SupplementGroup> query, string? supplementNameQuery, SupplementGroupSearchField searchField){
        if (string.IsNullOrWhiteSpace(supplementNameQuery))
        {
            return query;
        }

        string trimmedQuery = supplementNameQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return searchField switch
        {
            SupplementGroupSearchField.GroupName => query.Where(group =>
                group.Name != null && (EF.Functions.ILike(group.Name, prefixPattern) ||
                                       EF.Functions.ILike(group.Name, containsPattern) ||
                                       useTrigram &&
                                       EF.Functions.TrigramsSimilarity(group.Name, trimmedQuery) >= SimilarityThreshold)),

            SupplementGroupSearchField.SuplementName => query.Where(group => 
                group.Supplements.Any(supplement => 
                    EF.Functions.ILike(supplement.Name, prefixPattern) || 
                    EF.Functions.ILike(supplement.Name, containsPattern) || 
                    useTrigram && 
                    EF.Functions.TrigramsSimilarity(supplement.Name, trimmedQuery) >= SimilarityThreshold)),

            _ => query
        };
    }

    private static IQueryable<SupplementGroup> ApplySortAndOrder(IQueryable<SupplementGroup> query) => query.OrderBy(group => group.Name).ThenBy(group => group.Id);

    public async Task<List<SupplementGroup>> SearchAsync(
        string? supplementNameQuery,
        SupplementGroupSearchField searchField,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {

        // build query
        IQueryable<SupplementGroup> query = context.SupplementGroups.AsNoTracking();

        // apply includes
        query = ApplyIncludes(query);

        // apply search
        query = ApplySearch(query, supplementNameQuery, searchField);

        // apply sort and order
        query = ApplySortAndOrder(query);

        // return results
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? supplementNameQuery,
        SupplementGroupSearchField searchField,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<SupplementGroup> query = context.SupplementGroups.AsNoTracking();

        // apply includes
        query = ApplyIncludes(query);

        // apply search
        query = ApplySearch(query, supplementNameQuery, searchField);

        // return results
        return await query
            .CountAsync(cancellationToken);
    }

    public async Task<SupplementGroup?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await context.SupplementGroups
            .Include(group => group.Supplements)
            .FirstOrDefaultAsync(group => group.Id == id, cancellationToken);
    }

    public async Task<SupplementGroup?> GetByNameAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return await context.SupplementGroups
            .AsNoTracking()
            .Include(group => group.Supplements)
            .FirstOrDefaultAsync(group => group.Name == normalizedName, cancellationToken);
    }

    public async Task AddAsync(
        SupplementGroup group,
        CancellationToken cancellationToken)
    {
        await context.SupplementGroups.AddAsync(group, cancellationToken);
    }

    public void Remove(SupplementGroup group)
    {
        context.SupplementGroups.Remove(group);
    }

    public async Task<bool> IsInUseByInteractionsAsync(
        Guid supplementGroupId,
        CancellationToken cancellationToken)
    {
        // Placeholder for future interaction checks; current implementation is a simple existence guard.
        return await context.SupplementGroups
            .AsNoTracking()
            .AnyAsync(group => group.Id == supplementGroupId, cancellationToken);
    }
}
