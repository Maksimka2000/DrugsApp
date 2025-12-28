using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

namespace DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;

public interface IRouteOfAdministrationRepository
{
    Task<List<RouteOfAdministration>> SearchAsync(
        string? nameFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? nameFilter,
        CancellationToken cancellationToken);

    Task<RouteOfAdministration?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RouteOfAdministration?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(RouteOfAdministration routeOfAdministration, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RouteOfAdministration>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);
}


