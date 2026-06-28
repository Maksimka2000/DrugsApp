using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class UserProfilePersistenceAssertions
{
    internal static async Task AssertOnboardingCompletedAsync(
        IServiceProvider services,
        Guid userId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        UserProfile? profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        profile.Should().NotBeNull();
        profile!.OnboardingCompletedAtUtc.Should().NotBeNull();
    }

    internal static async Task AssertProfilePhotoStoredAsync(
        IServiceProvider services,
        Guid userId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        UserProfile? profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        profile.Should().NotBeNull();
        profile!.ProfilePhotoStorageKey.Should().NotBeNullOrWhiteSpace();
    }

    internal static async Task AssertReproductiveProfileExistsAsync(
        IServiceProvider services,
        Guid userId,
        bool expectedCurrentlyPregnant,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        ClientReproductiveHealthProfile? profile = await dbContext.ClientReproductiveHealthProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.UserId == userId, cancellationToken);

        profile.Should().NotBeNull();
        profile!.CurrentlyPregnant.Should().Be(expectedCurrentlyPregnant);
    }

    internal static async Task AssertReproductiveProfileMissingAsync(
        IServiceProvider services,
        Guid userId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        bool exists = await dbContext.ClientReproductiveHealthProfiles
            .AsNoTracking()
            .AnyAsync(entity => entity.UserId == userId, cancellationToken);

        exists.Should().BeFalse();
    }

    internal static async Task AssertHealthcareProviderProfileAsync(
        IServiceProvider services,
        Guid userId,
        HealthcareProviderProfession expectedProfession,
        string expectedWorkplace,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        HealthcareProviderProfile? profile = await dbContext.HealthcareProviderProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.UserId == userId, cancellationToken);

        profile.Should().NotBeNull();
        profile!.ProfessionCode.Should().Be(expectedProfession);
        profile.WorkplaceLabel.Should().Be(expectedWorkplace);
    }

    internal static async Task AssertHealthcareProviderProfileMissingAsync(
        IServiceProvider services,
        Guid userId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        bool exists = await dbContext.HealthcareProviderProfiles
            .AsNoTracking()
            .AnyAsync(entity => entity.UserId == userId, cancellationToken);

        exists.Should().BeFalse();
    }
}
