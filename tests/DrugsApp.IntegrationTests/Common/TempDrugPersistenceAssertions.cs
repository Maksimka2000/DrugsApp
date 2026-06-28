using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class TempDrugPersistenceAssertions
{
    internal static async Task AssertTempDrugStatusAsync(
        IServiceProvider services,
        Guid tempDrugId,
        TempDrugStatus expectedStatus,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        TempDrug? tempDrug = await dbContext.TempDrugs
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == tempDrugId, cancellationToken);

        tempDrug.Should().NotBeNull();
        tempDrug!.Status.Should().Be(expectedStatus);
    }

    internal static async Task AssertImportBatchExistsAsync(
        IServiceProvider services,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        bool exists = await dbContext.TempDrugImportBatches
            .AsNoTracking()
            .AnyAsync(batch => batch.Id == batchId, cancellationToken);

        exists.Should().BeTrue();
    }
}
