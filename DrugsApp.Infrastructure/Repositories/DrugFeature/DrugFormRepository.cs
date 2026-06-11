using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.DrugFeature;

public sealed class DrugFormRepository : IDrugFormRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public DrugFormRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<DrugForm?> GetByIdAsync(Guid drugFormId, CancellationToken cancellationToken)
    {
        return await _context.DrugForms
            .Include(df => df.RoutesOfAdministration)
            .Include(df => df.PharmaceuticalForms)
            .FirstOrDefaultAsync(df => df.Id == drugFormId, cancellationToken);
    }

    public async Task<List<DrugForm>> SearchFormsAsync(
        string? searchQuery,
        DrugFormSearchField searchField,
        DrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugForm> query = ApplyIncludes(_context.DrugForms.AsNoTracking());
        query = ApplySearchFilter(query, searchQuery, searchField);
        query = ApplyOrder(query, searchQuery, searchField, sortField, ascending);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountFormsAsync(
        string? searchQuery,
        DrugFormSearchField searchField,
        CancellationToken cancellationToken)
    {
        IQueryable<DrugForm> query = _context.DrugForms.AsNoTracking();
        query = ApplySearchFilter(query, searchQuery, searchField);

        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<DrugForm> ApplyIncludes(IQueryable<DrugForm> query) =>
        query
            .Include(df => df.Drug)
                .ThenInclude(drug => drug.ActiveSubstances)
            .Include(df => df.PharmaceuticalForms);

    private static IQueryable<DrugForm> ApplyOrder(
        IQueryable<DrugForm> query,
        string? searchQuery,
        DrugFormSearchField searchField,
        DrugSortField sortField,
        bool ascending)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery) && searchField is not DrugFormSearchField.None)
        {
            string trimmedQuery = searchQuery.Trim();
            string prefixPattern = $"{trimmedQuery}%";
            string containsPattern = $"%{trimmedQuery}%";
            bool useTrigram = trimmedQuery.Length >= 3;

            if (searchField == DrugFormSearchField.CommercialName)
            {
                return query
                    .OrderByDescending(df => EF.Functions.ILike(df.Drug.CommercialName, prefixPattern))
                    .ThenByDescending(df => EF.Functions.ILike(df.Drug.CommercialName, containsPattern))
                    .ThenByDescending(df =>
                        useTrigram
                            ? EF.Functions.TrigramsSimilarity(df.Drug.CommercialName, trimmedQuery)
                            : 0)
                    .ThenBy(df => df.Drug.CommercialName)
                    .ThenBy(df => df.ProductIdentifier)
                    .ThenBy(df => df.Id);
            }

            if (searchField == DrugFormSearchField.ActiveSubstance)
            {
                return query
                    .OrderByDescending(df => df.Drug.ActiveSubstances.Any(activeSubstance =>
                        activeSubstance.ActiveSubstanceName != null &&
                        EF.Functions.ILike(activeSubstance.ActiveSubstanceName, prefixPattern)))
                    .ThenByDescending(df => df.Drug.ActiveSubstances.Any(activeSubstance =>
                        activeSubstance.ActiveSubstanceName != null &&
                        EF.Functions.ILike(activeSubstance.ActiveSubstanceName, containsPattern)))
                    .ThenByDescending(df => df.Drug.ActiveSubstances.Any(activeSubstance =>
                        activeSubstance.ActiveSubstanceName != null &&
                        useTrigram &&
                        EF.Functions.TrigramsSimilarity(activeSubstance.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold))
                    .ThenBy(df => df.Drug.CommercialName)
                    .ThenBy(df => df.ProductIdentifier)
                    .ThenBy(df => df.Id);
            }
        }

        return (sortField, ascending) switch
        {
            (DrugSortField.CreatedAt, true) => query
                .OrderBy(df => df.Drug.CreatedAtUtc)
                .ThenBy(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id),
            (DrugSortField.CreatedAt, false) => query
                .OrderByDescending(df => df.Drug.CreatedAtUtc)
                .ThenBy(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id),

            (DrugSortField.UpdatedAt, true) => query
                .OrderBy(df => df.Drug.UpdatedAtUtc)
                .ThenBy(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id),
            (DrugSortField.UpdatedAt, false) => query
                .OrderByDescending(df => df.Drug.UpdatedAtUtc)
                .ThenBy(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id),

            (DrugSortField.CommercialName, false) => query
                .OrderByDescending(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id),

            _ => query
                .OrderBy(df => df.Drug.CommercialName)
                .ThenBy(df => df.ProductIdentifier)
                .ThenBy(df => df.Id)
        };
    }

    private static IQueryable<DrugForm> ApplySearchFilter(
        IQueryable<DrugForm> query,
        string? searchQuery,
        DrugFormSearchField searchField)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return query;
        }

        string trimmedQuery = searchQuery.Trim();
        string prefixPattern = $"{trimmedQuery}%";
        string containsPattern = $"%{trimmedQuery}%";
        bool useTrigram = trimmedQuery.Length >= 3;

        return searchField switch
        {
            DrugFormSearchField.CommercialName => query.Where(df =>
                EF.Functions.ILike(df.Drug.CommercialName, prefixPattern) ||
                EF.Functions.ILike(df.Drug.CommercialName, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(df.Drug.CommercialName, trimmedQuery) >= SimilarityThreshold),

            DrugFormSearchField.ActiveSubstance => query.Where(df =>
                df.Drug.ActiveSubstances.Any(activeSubstance =>
                    activeSubstance.ActiveSubstanceName != null &&
                    (EF.Functions.ILike(activeSubstance.ActiveSubstanceName, prefixPattern) ||
                     EF.Functions.ILike(activeSubstance.ActiveSubstanceName, containsPattern) ||
                     useTrigram &&
                     EF.Functions.TrigramsSimilarity(activeSubstance.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold))),

            _ => query
        };
    }
}
