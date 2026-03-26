using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.RouteOfAdministrationFeature;

public sealed class RouteOfAdministrationRepository : IRouteOfAdministrationRepository
{
    private const double SimilarityThreshold = 0.35;
    
    private readonly DrugsAppDbContext _context;

    public RouteOfAdministrationRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    

    public async Task<RouteOfAdministration?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.RoutesOfAdministration
            .FirstOrDefaultAsync(ra => ra.Id == id, cancellationToken);
    }

    public async Task<RouteOfAdministration?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.RoutesOfAdministration
            .AsNoTracking()
            .FirstOrDefaultAsync(ra => EF.Functions.ILike(ra.RouteOfAdministrationName, name), cancellationToken);
    }

    public async Task AddAsync(RouteOfAdministration routeOfAdministration, CancellationToken cancellationToken)
    {
        await _context.RoutesOfAdministration.AddAsync(routeOfAdministration, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RouteOfAdministration>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<RouteOfAdministration>();
        }

        return await _context.RoutesOfAdministration
            .Where(routeOfAdministration => ids.Contains(routeOfAdministration.Id))
            .ToListAsync(cancellationToken);
    }









    public async Task<List<RouteOfAdministration>> SearchAsync(
        string? searchQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<RouteOfAdministration> query = _context.RoutesOfAdministration.AsNoTracking();

        // apply filters

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply includes

        // apply sort
        query = ApplySort(query);

        // apply paging
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchQuery,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<RouteOfAdministration> query = _context.RoutesOfAdministration.AsNoTracking();

        // apply filters

        // apply search filter
        query = ApplySearchFilter(query, searchQuery);

        // apply counting
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<RouteOfAdministration> ApplySearchFilter(
        IQueryable<RouteOfAdministration> query,
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

        return query.Where(ra =>
            EF.Functions.ILike(ra.RouteOfAdministrationName, prefixPattern) ||
            EF.Functions.ILike(ra.RouteOfAdministrationName, containsPattern) ||
            useTrigram &&
            EF.Functions.TrigramsSimilarity(ra.RouteOfAdministrationName, trimmedQuery) >= SimilarityThreshold);
    }

    private static IQueryable<RouteOfAdministration> ApplySort(IQueryable<RouteOfAdministration> query) =>
        query.OrderByDescending(ra => ra.RouteOfAdministrationName);


}


