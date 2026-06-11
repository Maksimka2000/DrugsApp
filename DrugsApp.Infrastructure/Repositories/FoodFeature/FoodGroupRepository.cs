using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.FoodFeature;

public sealed class FoodGroupRepository : IFoodGroupRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public FoodGroupRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FoodGroup>> SearchAsyncByGroupName(
        string? searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<FoodGroup> query = _context.FoodGroups
            .AsNoTracking()
            .Include(group => group.Foods);

        query = ApplyGroupNameSearchFilter(query, searchQuery);
        query = query
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FoodGroup>> SearchAsyncByMemberFoodName(
        string foodNameQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<FoodGroup> query = _context.FoodGroups
            .AsNoTracking()
            .Include(group => group.Foods);

        query = ApplyMemberFoodNameSearchFilter(query, foodNameQuery);
        query = query
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByGroupNameAsync(
        string? searchQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<FoodGroup> query = _context.FoodGroups.AsNoTracking();

        query = ApplyGroupNameSearchFilter(query, searchQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountByMemberFoodNameAsync(
        string foodNameQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<FoodGroup> query = _context.FoodGroups.AsNoTracking();

        query = ApplyMemberFoodNameSearchFilter(query, foodNameQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<FoodGroup?> GetByIdAsync(Guid foodGroupId, CancellationToken cancellationToken)
    {
        return await _context.FoodGroups
            .Include(group => group.Foods)
            .FirstOrDefaultAsync(group => group.Id == foodGroupId, cancellationToken);
    }

    public async Task<FoodGroup?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        string trimmedName = name.Trim();

        return await _context.FoodGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(group => EF.Functions.ILike(group.Name, trimmedName), cancellationToken);
    }

    public async Task AddAsync(FoodGroup foodGroup, CancellationToken cancellationToken)
    {
        await _context.FoodGroups.AddAsync(foodGroup, cancellationToken);
    }

    public void Remove(FoodGroup foodGroup)
    {
        _context.FoodGroups.Remove(foodGroup);
    }

    public async Task<bool> ExistsWithMembersAsync(
        IReadOnlyCollection<Guid> foodIds,
        CancellationToken cancellationToken)
    {
        if (foodIds.Count == 0)
        {
            return false;
        }

        var requestedIds = foodIds.ToHashSet();

        return await _context.FoodGroups
            .AsNoTracking()
            .Where(group => group.Foods.Count == requestedIds.Count)
            .AnyAsync(
                group => group.Foods.All(food => requestedIds.Contains(food.Id)),
                cancellationToken);
    }

    public Task<bool> IsReferencedByInteractionsAsync(Guid foodGroupId, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    private static IQueryable<FoodGroup> ApplyGroupNameSearchFilter(
        IQueryable<FoodGroup> query,
        string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return query;
        }

        string trimmedQuery = searchQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(group =>
            EF.Functions.ILike(group.Name, prefixPattern) ||
            EF.Functions.ILike(group.Name, containsPattern) ||
            useTrigram && EF.Functions.TrigramsSimilarity(group.Name, trimmedQuery) >= SimilarityThreshold);
    }

    private static IQueryable<FoodGroup> ApplyMemberFoodNameSearchFilter(
        IQueryable<FoodGroup> query,
        string foodNameQuery)
    {
        if (string.IsNullOrWhiteSpace(foodNameQuery))
        {
            return query;
        }

        string trimmedQuery = foodNameQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(group =>
            group.Foods.Any(food =>
                EF.Functions.ILike(food.SearchText, prefixPattern) ||
                EF.Functions.ILike(food.SearchText, containsPattern) ||
                useTrigram && EF.Functions.TrigramsSimilarity(food.SearchText, trimmedQuery) >= SimilarityThreshold));
    }

    public async Task<List<FoodGroup>> GetByIdsAsync(
        IReadOnlyCollection<Guid> foodGroupIds,
        CancellationToken cancellationToken)
    {
        if (foodGroupIds.Count == 0)
        {
            return new List<FoodGroup>();
        }

        return await _context.FoodGroups
            .AsNoTracking()
            .Include(group => group.Foods)
            .Where(group => foodGroupIds.Contains(group.Id))
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id)
            .ToListAsync(cancellationToken);
    }
}
