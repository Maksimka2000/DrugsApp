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
        IQueryable<RouteOfAdministration> query = _context.RoutesOfAdministration.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(ra => EF.Functions.ILike(ra.RouteOfAdministrationName, $"%{nameFilter}%"));
        }

        query = query.OrderBy(ra => ra.RouteOfAdministrationName);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<RouteOfAdministration> query = _context.RoutesOfAdministration.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(ra => EF.Functions.ILike(ra.RouteOfAdministrationName, $"%{nameFilter}%"));
        }

        return await query.CountAsync(cancellationToken);
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
}


