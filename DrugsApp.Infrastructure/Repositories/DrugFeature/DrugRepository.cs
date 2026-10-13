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

public sealed class DrugRepository : IDrugRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public DrugRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<Drug?> GetByIdAsync(Guid drugId, CancellationToken cancellationToken)
    {
        return await ApplyIncludes(_context.Drugs)
            .FirstOrDefaultAsync(d => d.Id == drugId, cancellationToken);
    }

    public async Task AddAsync(Drug drug, CancellationToken cancellationToken)
    {
        await _context.Drugs.AddAsync(drug, cancellationToken);
    }

    public async Task<List<Drug>> SearchSimilarCandidatesAsync(
        string commercialName,
        string typeOfProduct,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commercialName) || string.IsNullOrWhiteSpace(typeOfProduct))
        {
            return new List<Drug>();
        }

        string trimmedCommercialName = commercialName.Trim();
        string trimmedTypeOfProduct = typeOfProduct.Trim();

        bool useTrigram = trimmedCommercialName.Length >= 3;
        string containsPattern = $"%{trimmedCommercialName}%";

        IQueryable<Drug> query = _context.Drugs
            .AsNoTracking()
            .Where(d => EF.Functions.ILike(d.TypeOfProduct, trimmedTypeOfProduct))
            .Where(d =>
                EF.Functions.ILike(d.CommercialName, containsPattern) ||
                useTrigram && EF.Functions.TrigramsSimilarity(d.CommercialName, trimmedCommercialName) >= SimilarityThreshold);

        query = ApplyIncludes(query);

        
        query = query
                .OrderBy(d => d.CommercialName)
                .ThenBy(d => d.Id);
    

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Drug>> SearchAsync(
        string? searchQuery,
        IReadOnlyCollection<DrugType>? typeOfDrugsFilter,
        DrugSearchField searchField,
        DrugSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<Drug> query = _context.Drugs.AsNoTracking();

        // apply filters
        query = ApplyTypeFilter(query, typeOfDrugsFilter);

        // apply search filter
        query = ApplySearchFilter(query, searchQuery, searchField);

        // apply includes
        query = ApplyIncludes(query);

        // apply sort
        query = ApplySort(query, sortField, ascending);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchQuery,
        IReadOnlyCollection<DrugType>? typeOfDrugsFilter,
        DrugSearchField searchField,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<Drug> query = _context.Drugs.AsNoTracking();

        // apply filters
        query = ApplyTypeFilter(query, typeOfDrugsFilter);

        // apply search filter
        query = ApplySearchFilter(query, searchQuery, searchField);

        return await query.CountAsync(cancellationToken);
    }









    private static IQueryable<Drug> ApplyIncludes(IQueryable<Drug> query) =>
        query
            .Include(d => d.ActiveSubstances)
                .ThenInclude(s => s.AlcoholInteractionOverride)
            .Include(d => d.ActiveSubstances)
                .ThenInclude(s => s.OralMedicationIntakeGuidelines)
            .Include(d => d.DrugForms)
                .ThenInclude(df => df.RoutesOfAdministration)
            .Include(d => d.DrugForms)
                .ThenInclude(df => df.PharmaceuticalForms);






    private static IQueryable<Drug> ApplyTypeFilter(
        IQueryable<Drug> query,
        IReadOnlyCollection<DrugType>? typeOfDrugsFilter)
    {
        if (typeOfDrugsFilter is null || typeOfDrugsFilter.Count == 0)
        {
            return query;
        }

        return query.Where(d => typeOfDrugsFilter.Contains(d.TypeOfDrug));
    }





    private static IQueryable<Drug> ApplySort(
        IQueryable<Drug> query,
        DrugSortField sortField,
        bool ascending)
    {
        return (sortField, ascending) switch
        {
            (DrugSortField.CreatedAt, true) => query
                .OrderBy(d => d.CreatedAtUtc)
                .ThenBy(d => d.Id),
            (DrugSortField.CreatedAt, false) => query
                .OrderByDescending(d => d.CreatedAtUtc)
                .ThenBy(d => d.Id),

            (DrugSortField.UpdatedAt, true) => query
                .OrderBy(d => d.UpdatedAtUtc)
                .ThenBy(d => d.Id),
            (DrugSortField.UpdatedAt, false) => query
                .OrderByDescending(d => d.UpdatedAtUtc)
                .ThenBy(d => d.Id),

            (DrugSortField.CommercialName, true) => query
                .OrderBy(d => d.CommercialName)
                .ThenBy(d => d.Id),
            (DrugSortField.CommercialName, false) => query
                .OrderByDescending(d => d.CommercialName)
                .ThenBy(d => d.Id),

            _ => query.OrderBy(d => d.CommercialName).ThenBy(d => d.Id)
        };
    }

    private static IQueryable<Drug> ApplySearchFilter(
        IQueryable<Drug> query,
        string? searchQuery,
        DrugSearchField searchField)
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
            DrugSearchField.CommercialName => query.Where(d =>
                EF.Functions.ILike(d.CommercialName, prefixPattern) ||
                EF.Functions.ILike(d.CommercialName, containsPattern) ||
                useTrigram && 
                EF.Functions.TrigramsSimilarity(d.CommercialName, trimmedQuery) >= SimilarityThreshold),

            DrugSearchField.ActiveSubstance => query.Where(d =>
                d.ActiveSubstances.Any(a =>
                    a.ActiveSubstanceName != null &&
                    (EF.Functions.ILike(a.ActiveSubstanceName, prefixPattern) ||
                     EF.Functions.ILike(a.ActiveSubstanceName, containsPattern) ||
                     useTrigram && EF.Functions.TrigramsSimilarity(a.ActiveSubstanceName, trimmedQuery) >= SimilarityThreshold))),

            DrugSearchField.AtcCode => query.Where(d =>
                d.DrugForms.Any(df =>
                    df.AtcCode != null &&
                    (EF.Functions.ILike(df.AtcCode, prefixPattern) ||
                     EF.Functions.ILike(df.AtcCode, containsPattern) ||
                     useTrigram && EF.Functions.TrigramsSimilarity(df.AtcCode, trimmedQuery) >= SimilarityThreshold))),

            DrugSearchField.RouteOfAdministration => query.Where(d =>
                d.DrugForms.Any(df =>
                    df.RoutesOfAdministration.Any(roa =>
                        EF.Functions.ILike(roa.RouteOfAdministrationName, prefixPattern) ||
                        EF.Functions.ILike(roa.RouteOfAdministrationName, containsPattern) ||
                        useTrigram && EF.Functions.TrigramsSimilarity(roa.RouteOfAdministrationName, trimmedQuery) >= SimilarityThreshold))),

            DrugSearchField.PharmaceuticalForm => query.Where(d =>
                d.DrugForms.Any(df =>
                    df.PharmaceuticalForms.Any(pf =>
                        EF.Functions.ILike(pf.FormulationName, prefixPattern) ||
                        EF.Functions.ILike(pf.FormulationName, containsPattern) ||
                        useTrigram && EF.Functions.TrigramsSimilarity(pf.FormulationName, trimmedQuery) >= SimilarityThreshold))),

            DrugSearchField.ProductIdentifier => ApplyProductIdentifierFilter(query, trimmedQuery),

            _ => query
        };
    }

    private static IQueryable<Drug> ApplyProductIdentifierFilter(
        IQueryable<Drug> query,
        string searchQuery)
    {
        if (!int.TryParse(searchQuery, out int numericIdentifier))
        {
            return query.Where(_ => false);
        }

        return query.Where(d =>
            d.DrugForms.Any(df => df.ProductIdentifier == numericIdentifier));
    }

    public async Task<List<Drug>> GetByDrugFormIdsAsync(
        IReadOnlyCollection<Guid> drugFormIds,
        CancellationToken cancellationToken)
    {
        if (drugFormIds.Count == 0)
        {
            return new List<Drug>();
        }

        return await _context.Drugs
            .AsNoTracking()
            .Include(drug => drug.ActiveSubstances)
                .ThenInclude(substance => substance.AlcoholInteractionOverride)
            .Include(drug => drug.ActiveSubstances)
                .ThenInclude(substance => substance.OralMedicationIntakeGuidelines)
            .Include(drug => drug.DrugForms.Where(form => drugFormIds.Contains(form.Id)))
                .ThenInclude(form => form.RoutesOfAdministration)
            .Include(drug => drug.DrugForms.Where(form => drugFormIds.Contains(form.Id)))
                .ThenInclude(form => form.PharmaceuticalForms)
            .AsSplitQuery()
            .Where(drug => drug.DrugForms.Any(form => drugFormIds.Contains(form.Id)))
            .OrderBy(drug => drug.CommercialName)
            .ThenBy(drug => drug.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Drug>> ListByActiveSubstanceIdsAsync(
        IReadOnlyCollection<Guid> activeSubstanceIds,
        Guid? excludeDrugId,
        CancellationToken cancellationToken)
    {
        if (activeSubstanceIds.Count == 0)
        {
            return new List<Drug>();
        }

        IQueryable<Drug> query = ApplyIncludes(_context.Drugs.AsNoTracking())
            .Where(drug => drug.ActiveSubstances.Any(substance => activeSubstanceIds.Contains(substance.Id)));

        if (excludeDrugId.HasValue)
        {
            query = query.Where(drug => drug.Id != excludeDrugId.Value);
        }

        return await query
            .OrderBy(drug => drug.CommercialName)
            .ThenBy(drug => drug.Id)
            .ToListAsync(cancellationToken);
    }

}
