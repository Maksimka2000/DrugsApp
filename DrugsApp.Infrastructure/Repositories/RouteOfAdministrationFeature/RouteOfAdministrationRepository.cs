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
    private readonly DrugsAppDbContext _context;

    public RouteOfAdministrationRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RouteOfAdministration>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<RouteOfAdministration> query = BuildSearchQueryable(nameFilter);

        List<RouteOfAdministration> result = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<RouteOfAdministration> query = BuildSearchQueryable(nameFilter);

        int result = await query.CountAsync(cancellationToken);

        return result;
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
    private IQueryable<RouteOfAdministration> BuildSearchQueryable(string? nameFilter)
    {
        string? trimmedFilter = nameFilter?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedFilter))
        {
            return _context.RoutesOfAdministration
                .AsNoTracking()
                .OrderBy(ra => ra.RouteOfAdministrationName);
        }

        if (trimmedFilter.Length < 3)
        {
            return _context.RoutesOfAdministration
                .AsNoTracking()
                .Where(ra => EF.Functions.ILike(ra.RouteOfAdministrationName, $"%{trimmedFilter}%"))
                .OrderBy(ra => ra.RouteOfAdministrationName);
        }

        return _context.RoutesOfAdministration
            .FromSqlInterpolated($"""
                SELECT * FROM "RoutesOfAdministration" ra
                WHERE ra."RouteOfAdministrationName" ILIKE '%' || {trimmedFilter} || '%'
                   OR ra."RouteOfAdministrationName" % {trimmedFilter}
                ORDER BY similarity(ra."RouteOfAdministrationName", {trimmedFilter}) DESC,
                         ra."RouteOfAdministrationName"
                """)
            .AsNoTracking();
    }
}


