using DrugsApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Infrastructure.Extensions;

/// <summary>
/// Host startup helpers for applying EF Core migrations.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending EF Core migrations for the configured database.
    /// </summary>
    /// <param name="app">Application builder.</param>
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        dbContext.Database.Migrate();
    }
}


