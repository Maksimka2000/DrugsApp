using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.RegularDrugsFeature;

public sealed class ClientRegularDrugRepository(DrugsAppDbContext _context) : IClientRegularDrugRepository
{
    public async Task<List<ClientRegularDrug>> SearchLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = BuildLiveQuery(userId, drugNameQuery, startDate);

        query = ApplyIncludes(query)
            .OrderBy(row => row.Status == RegularDrugStatus.Scheduled ? 1 : 0)
            .ThenBy(row => row.Drug.CommercialName.ToLower())
            .ThenBy(row => row.StartDate ?? DateOnly.MinValue)
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientRegularDrug>> ListActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .Where(row => row.UserId == userId && row.Status == RegularDrugStatus.Active);

        return await ApplyIncludes(query)
            .OrderBy(row => row.Drug.CommercialName.ToLower())
            .ThenBy(row => row.StartDate ?? DateOnly.MinValue)
            .ThenBy(row => row.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = BuildLiveQuery(userId, drugNameQuery, startDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<List<ClientRegularDrug>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = BuildHistoryQuery(userId, drugNameQuery, startDate, endDate);

        query = ApplyIncludes(query)
            .OrderByDescending(row => row.ArchivedAtUtc ?? DateTime.MinValue)
            .ThenBy(row => row.Drug.CommercialName.ToLower())
            .ThenBy(row => row.DrugFormId)
            .ThenBy(row => row.Id);

        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = BuildHistoryQuery(userId, drugNameQuery, startDate, endDate);

        return await query
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<ClientRegularDrug?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .Where(row => row.UserId == userId && row.Id == id);

        query = ApplyIncludes(query);

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsLiveForUserWithDrugIdAsync(
        Guid userId,
        Guid drugId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .AsNoTracking()
            .Where(row =>
                row.UserId == userId &&
                row.DrugId == drugId &&
                (row.Status == RegularDrugStatus.Scheduled || row.Status == RegularDrugStatus.Active));

        if (excludeId.HasValue)
        {
            query = query.Where(row => row.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ClientRegularDrug clientRegularDrug, CancellationToken cancellationToken)
    {
        await _context.Set<ClientRegularDrug>().AddAsync(clientRegularDrug, cancellationToken);
    }

    public void Remove(ClientRegularDrug clientRegularDrug)
    {
        _context.Set<ClientRegularDrug>().Remove(clientRegularDrug);
    }

    public async Task<int> DeleteExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ClientRegularDrug> expiredRows = await _context.Set<ClientRegularDrug>()
            .Where(row => row.UserId == userId && row.Status == RegularDrugStatus.Expired)
            .ToListAsync(cancellationToken);

        _context.Set<ClientRegularDrug>().RemoveRange(expiredRows);

        return expiredRows.Count;
    }

    public async Task<List<ClientRegularDrug>> FindEndedLiveRowsAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .Where(row =>
                (row.Status == RegularDrugStatus.Scheduled || row.Status == RegularDrugStatus.Active) &&
                row.EndDate != null &&
                row.EndDate < asOfDate);

        query = ApplyIncludes(query);

        return await query
            .OrderBy(row => row.EndDate)
            .ThenBy(row => row.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<ClientRegularDrug> BuildLiveQuery(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .Where(row =>
                row.UserId == userId &&
                (row.Status == RegularDrugStatus.Scheduled || row.Status == RegularDrugStatus.Active));

        query = ApplyDrugNameFilter(query, drugNameQuery);

        if (startDate.HasValue)
        {
            query = query.Where(row => row.StartDate == startDate.Value);
        }

        return query;
    }

    private IQueryable<ClientRegularDrug> BuildHistoryQuery(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        IQueryable<ClientRegularDrug> query = _context.Set<ClientRegularDrug>()
            .Where(row => row.UserId == userId && row.Status == RegularDrugStatus.Expired);

        query = ApplyDrugNameFilter(query, drugNameQuery);

        if (startDate.HasValue)
        {
            query = query.Where(row => row.StartDate == startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(row => row.EndDate == endDate.Value);
        }

        return query;
    }

    private static IQueryable<ClientRegularDrug> ApplyDrugNameFilter(
        IQueryable<ClientRegularDrug> query,
        string? drugNameQuery)
    {
        if (string.IsNullOrWhiteSpace(drugNameQuery))
        {
            return query;
        }

        string trimmedQuery = drugNameQuery.Trim();
        string containsPattern = $"%{trimmedQuery}%";

        return query.Where(row =>
            EF.Functions.ILike(row.Drug.CommercialName, containsPattern) ||
            row.Drug.ActiveSubstances.Any(activeSubstance =>
                EF.Functions.ILike(activeSubstance.ActiveSubstanceName, containsPattern)));
    }

    private static IQueryable<ClientRegularDrug> ApplyIncludes(IQueryable<ClientRegularDrug> query) =>
        query
            .Include(row => row.IntakeTimes)
            .Include(row => row.Drug)
                .ThenInclude(drug => drug.ActiveSubstances)
                    .ThenInclude(substance => substance.AlcoholInteractionOverride)
            .Include(row => row.Drug)
                .ThenInclude(drug => drug.ActiveSubstances)
                    .ThenInclude(substance => substance.OralMedicationIntakeGuidelines)
            .Include(row => row.DrugForm)
                .ThenInclude(drugForm => drugForm.PharmaceuticalForms)
            .Include(row => row.DrugForm)
                .ThenInclude(drugForm => drugForm.RoutesOfAdministration)
            .AsSplitQuery();
}
