using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class RouteOfAdministrationPersistenceAssertions
{
    internal static async Task AssertRouteAsync(
        IServiceProvider services,
        Guid routeOfAdministrationId,
        string expectedName,
        bool expectedIsOralIntake,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        RouteOfAdministration? route = await dbContext.RoutesOfAdministration
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == routeOfAdministrationId, cancellationToken);

        route.Should().NotBeNull();
        route!.RouteOfAdministrationName.Should().Be(expectedName);
        route.IsOralIntake.Should().Be(expectedIsOralIntake);
    }
}
