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

public sealed class ClientDrugAllergyRepository(DrugsAppDbContext _context) : IClientDrugAllergyRepository
{
    public async Task<List<ClientDrugAllergy>> SearchAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientDrugAllergy> query = ApplyCatalogIncludes(_context.ClientDrugAllergies.AsNoTracking())
            .Where(x => x.UserId == userId);

        query = ApplySort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientDrugAllergy>> ListAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientDrugAllergy> query = ApplyCatalogIncludes(_context.ClientDrugAllergies.AsNoTracking())
            .Where(x => x.UserId == userId);

        return await ApplySort(query).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientDrugAllergies
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ClientDrugAllergy?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await ApplyCatalogIncludes(_context.ClientDrugAllergies)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ClientDrugAllergy allergy, CancellationToken cancellationToken)
    {
        await _context.ClientDrugAllergies.AddAsync(allergy, cancellationToken);
    }

    public void Remove(ClientDrugAllergy allergy)
    {
        _context.ClientDrugAllergies.Remove(allergy);
    }

    public async Task<bool> ExistsByDrugFormIdAsync(
        Guid userId,
        Guid drugFormId,
        CancellationToken cancellationToken)
    {
        return await _context.ClientDrugAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.DrugFormId == drugFormId,
                cancellationToken);
    }

    public async Task<bool> ExistsCustomNameNormalizedAsync(
        Guid userId,
        string customDrugNameNormalized,
        CancellationToken cancellationToken)
    {
        return await _context.ClientDrugAllergies
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Custom &&
                     x.CustomDrugNameNormalized == customDrugNameNormalized,
                cancellationToken);
    }

    public async Task<bool> ExistsDatabaseDisplayNameNormalizedAsync(
        Guid userId,
        string drugDisplayNameNormalized,
        CancellationToken cancellationToken)
    {
        return await ApplyCatalogIncludes(_context.ClientDrugAllergies.AsNoTracking())
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Source == AllergyRecordSource.Database &&
                     x.DrugForm != null &&
                     x.DrugForm.Drug != null &&
                     x.DrugForm.Drug.CommercialName.ToLower() == drugDisplayNameNormalized,
                cancellationToken);
    }

    public async Task<bool> HasActiveSubstanceOverlapAsync(
        Guid userId,
        Guid candidateDrugId,
        CancellationToken cancellationToken)
    {
        List<Guid> candidateSubstanceIds = await _context.Drugs
            .AsNoTracking()
            .Where(drug => drug.Id == candidateDrugId)
            .SelectMany(drug => drug.ActiveSubstances)
            .Select(activeSubstance => activeSubstance.Id)
            .ToListAsync(cancellationToken);

        if (candidateSubstanceIds.Count == 0)
        {
            return false;
        }

        List<Guid> existingDrugIds = await _context.ClientDrugAllergies
            .AsNoTracking()
            .Where(allergy =>
                allergy.UserId == userId &&
                allergy.Source == AllergyRecordSource.Database &&
                allergy.DrugFormId != null)
            .Join(
                _context.DrugForms.AsNoTracking(),
                allergy => allergy.DrugFormId,
                drugForm => drugForm.Id,
                (_, drugForm) => drugForm.DrugId)
            .ToListAsync(cancellationToken);

        if (existingDrugIds.Count == 0)
        {
            return false;
        }

        return await _context.Drugs
            .AsNoTracking()
            .Where(drug => existingDrugIds.Contains(drug.Id))
            .SelectMany(drug => drug.ActiveSubstances)
            .AnyAsync(
                activeSubstance => candidateSubstanceIds.Contains(activeSubstance.Id),
                cancellationToken);
    }

    private static IQueryable<ClientDrugAllergy> ApplyCatalogIncludes(IQueryable<ClientDrugAllergy> query) =>
        query
            .Include(allergy => allergy.DrugForm)
                .ThenInclude(drugForm => drugForm!.Drug)
                    .ThenInclude(drug => drug.ActiveSubstances)
            .Include(allergy => allergy.DrugForm)
                .ThenInclude(drugForm => drugForm!.PharmaceuticalForms);

    private static IQueryable<ClientDrugAllergy> ApplySort(IQueryable<ClientDrugAllergy> query)
    {
        return query
            .OrderBy(allergy =>
                allergy.CustomDrugName != null
                    ? allergy.CustomDrugName.ToLower()
                    : allergy.DrugForm != null && allergy.DrugForm.Drug != null
                        ? allergy.DrugForm.Drug.CommercialName.ToLower()
                        : string.Empty)
            .ThenBy(allergy => allergy.Id);
    }
}
