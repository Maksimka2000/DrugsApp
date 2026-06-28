using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class InteractionPersistenceAssertions
{
    internal static async Task AssertSubstanceFoodPartiesAndExpandedAsync(
        IServiceProvider services,
        Guid interactionId,
        Guid substanceId,
        Guid foodId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        List<InteractionParty> parties = await dbContext.InteractionParties
            .AsNoTracking()
            .Where(party => party.InteractionId == interactionId)
            .ToListAsync(cancellationToken);

        parties.Should().HaveCount(2);

        InteractionParty substanceParty = parties.Single(party =>
            party.PartyType == InteractionType.Substance && party.ActiveSubstanceId == substanceId);
        substanceParty.InteractionId.Should().Be(interactionId);

        InteractionParty foodParty = parties.Single(party =>
            party.PartyType == InteractionType.Food && party.FoodId == foodId);
        foodParty.InteractionId.Should().Be(interactionId);

        List<InteractionExpanded> expandedRows = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(row => row.InteractionId == interactionId)
            .ToListAsync(cancellationToken);

        expandedRows.Should().ContainSingle();

        InteractionExpanded expanded = expandedRows[0];
        expanded.PartyTypeA.Should().Be(ShortInteractionType.Substance);
        expanded.PartyIdA.Should().Be(substanceId);
        expanded.PartyTypeB.Should().Be(ShortInteractionType.Food);
        expanded.PartyIdB.Should().Be(foodId);
    }

    internal static async Task AssertInteractionFullyRemovedAsync(
        IServiceProvider services,
        Guid interactionId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        bool interactionExists = await dbContext.Interactions
            .AsNoTracking()
            .AnyAsync(interaction => interaction.Id == interactionId, cancellationToken);

        int partyCount = await dbContext.InteractionParties
            .AsNoTracking()
            .CountAsync(party => party.InteractionId == interactionId, cancellationToken);

        int expandedCount = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .CountAsync(row => row.InteractionId == interactionId, cancellationToken);

        interactionExists.Should().BeFalse();
        partyCount.Should().Be(0);
        expandedCount.Should().Be(0);
    }

    internal static async Task<List<InteractionExpanded>> GetExpandedRowsAsync(
        IServiceProvider services,
        Guid interactionId,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        return await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(row => row.InteractionId == interactionId)
            .ToListAsync(cancellationToken);
    }

    internal static void AssertCanonicalSubstancePairExists(
        IReadOnlyCollection<InteractionExpanded> rows,
        Guid firstSubstanceId,
        Guid secondSubstanceId)
    {
        (Guid partyIdA, Guid partyIdB) = CanonicalizeSubstancePair(firstSubstanceId, secondSubstanceId);

        rows.Should().Contain(row =>
            row.PartyTypeA == ShortInteractionType.Substance &&
            row.PartyIdA == partyIdA &&
            row.PartyTypeB == ShortInteractionType.Substance &&
            row.PartyIdB == partyIdB);
    }

    internal static void AssertCanonicalSubstancePairMissing(
        IReadOnlyCollection<InteractionExpanded> rows,
        Guid firstSubstanceId,
        Guid secondSubstanceId)
    {
        (Guid partyIdA, Guid partyIdB) = CanonicalizeSubstancePair(firstSubstanceId, secondSubstanceId);

        rows.Should().NotContain(row =>
            row.PartyTypeA == ShortInteractionType.Substance &&
            row.PartyIdA == partyIdA &&
            row.PartyTypeB == ShortInteractionType.Substance &&
            row.PartyIdB == partyIdB);
    }

    internal static void AssertCanonicalSubstanceSupplementPairExists(
        IReadOnlyCollection<InteractionExpanded> rows,
        Guid substanceId,
        Guid supplementId)
    {
        rows.Should().Contain(row =>
            row.PartyTypeA == ShortInteractionType.Substance &&
            row.PartyIdA == substanceId &&
            row.PartyTypeB == ShortInteractionType.Supplement &&
            row.PartyIdB == supplementId);
    }

    internal static void AssertCanonicalSubstanceSupplementPairMissing(
        IReadOnlyCollection<InteractionExpanded> rows,
        Guid substanceId,
        Guid supplementId)
    {
        rows.Should().NotContain(row =>
            row.PartyTypeA == ShortInteractionType.Substance &&
            row.PartyIdA == substanceId &&
            row.PartyTypeB == ShortInteractionType.Supplement &&
            row.PartyIdB == supplementId);
    }

    private static (Guid PartyIdA, Guid PartyIdB) CanonicalizeSubstancePair(Guid firstSubstanceId, Guid secondSubstanceId)
    {
        return firstSubstanceId.CompareTo(secondSubstanceId) <= 0
            ? (firstSubstanceId, secondSubstanceId)
            : (secondSubstanceId, firstSubstanceId);
    }
}
