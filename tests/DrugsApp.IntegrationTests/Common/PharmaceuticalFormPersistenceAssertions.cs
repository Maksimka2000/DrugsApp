using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class PharmaceuticalFormPersistenceAssertions
{
    internal static async Task AssertFormulationNameAsync(
        IServiceProvider services,
        Guid pharmaceuticalFormId,
        string expectedFormulationName,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        PharmaceuticalForm? form = await dbContext.PharmaceuticalForms
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == pharmaceuticalFormId, cancellationToken);

        form.Should().NotBeNull();
        form!.FormulationName.Should().Be(expectedFormulationName);
    }
}
