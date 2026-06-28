using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.FoodFeature;

public sealed class FoodRepository : IFoodRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public FoodRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Food>> SearchAsync(
        string? searchText,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<Food> query = _context.Foods.AsNoTracking();

        query = ApplySearchFilter(query, searchText);
        query = ApplySort(query, searchText);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchText,
        CancellationToken cancellationToken)
    {
        IQueryable<Food> query = _context.Foods.AsNoTracking();

        query = ApplySearchFilter(query, searchText);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Food?> GetByIdAsync(Guid foodId, CancellationToken cancellationToken)
    {
        return await _context.Foods
            .FirstOrDefaultAsync(food => food.Id == foodId, cancellationToken);
    }

    public async Task<Food?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        string trimmedName = name.Trim();

        return await _context.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(food => EF.Functions.ILike(food.Name, trimmedName), cancellationToken);
    }

    public async Task<List<Food>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new List<Food>();
        }

        return await _context.Foods
            .Where(food => ids.Contains(food.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsReferencedAsync(Guid foodId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> groupIds = await GetFoodGroupIdsContainingAsync(foodId, cancellationToken);
        IReadOnlyCollection<Guid> interactionIds = await GetInteractionIdsReferencingAsync(foodId, cancellationToken);

        return groupIds.Count > 0 || interactionIds.Count > 0;
    }

    public async Task AddAsync(Food food, CancellationToken cancellationToken)
    {
        await _context.Foods.AddAsync(food, cancellationToken);
    }

    public void Remove(Food food)
    {
        _context.Foods.Remove(food);
    }

    public async Task<IReadOnlyCollection<Guid>> GetFoodGroupIdsContainingAsync(
        Guid foodId,
        CancellationToken cancellationToken)
    {
        if (foodId == Guid.Empty)
        {
            return Array.Empty<Guid>();
        }

        List<Guid> groupIds = await _context.FoodGroups
            .AsNoTracking()
            .Where(group => group.Foods.Any(food => food.Id == foodId))
            .Select(group => group.Id)
            .ToListAsync(cancellationToken);

        return groupIds;
    }

    public async Task<List<FoodGroup>> ListGroupsByFoodIdAsync(
        Guid foodId,
        CancellationToken cancellationToken)
    {
        if (foodId == Guid.Empty)
        {
            return [];
        }

        return await _context.FoodGroups
            .AsNoTracking()
            .Where(group => group.Foods.Any(food => food.Id == foodId))
            .OrderBy(group => group.Name)
            .ThenBy(group => group.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetInteractionIdsReferencingAsync(
        Guid foodId,
        CancellationToken cancellationToken)
    {
        if (foodId == Guid.Empty)
        {
            return Array.Empty<Guid>();
        }

        List<Guid> interactionIdsFromParties = await _context.InteractionParties
            .AsNoTracking()
            .Where(party =>
                party.PartyType == InteractionType.Food &&
                party.FoodId == foodId)
            .Select(party => party.InteractionId)
            .ToListAsync(cancellationToken);

        List<Guid> interactionIdsFromExpanded = await _context.InteractionExpandedRows
            .AsNoTracking()
            .Where(row =>
                row.PartyTypeA == ShortInteractionType.Food && row.PartyIdA == foodId ||
                row.PartyTypeB == ShortInteractionType.Food && row.PartyIdB == foodId)
            .Select(row => row.InteractionId)
            .ToListAsync(cancellationToken);

        return interactionIdsFromParties
            .Concat(interactionIdsFromExpanded)
            .Distinct()
            .ToList();
    }

    private static IQueryable<Food> ApplySearchFilter(
        IQueryable<Food> query,
        string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        string trimmedQuery = searchText.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return query.Where(food =>
            EF.Functions.ILike(food.SearchText, prefixPattern) ||
            EF.Functions.ILike(food.SearchText, containsPattern) ||
            useTrigram && EF.Functions.TrigramsSimilarity(food.SearchText, trimmedQuery) >= SimilarityThreshold);
    }

    private static IQueryable<Food> ApplySort(
        IQueryable<Food> query,
        string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query
                .OrderBy(food => food.Name)
                .ThenBy(food => food.Id);
        }

        string trimmedQuery = searchText.Trim();
        bool useTrigram = trimmedQuery.Length >= 3;

        return query
            .OrderByDescending(food =>
                useTrigram ? EF.Functions.TrigramsSimilarity(food.SearchText, trimmedQuery) : 0)
            .ThenBy(food => food.Name)
            .ThenBy(food => food.Id);
    }
}
