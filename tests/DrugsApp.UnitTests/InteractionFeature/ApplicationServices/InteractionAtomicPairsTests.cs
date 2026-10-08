using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using FluentAssertions;

namespace DrugsApp.UnitTests.InteractionFeature.ApplicationServices;

public sealed class InteractionAtomicPairsTests
{
    private static readonly Guid IdA = Guid.Parse("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid IdB = Guid.Parse("00000000-0000-0000-0000-00000000000b");
    private static readonly Guid IdC = Guid.Parse("00000000-0000-0000-0000-00000000000c");

    [Fact]
    public void BuildPairs_WhenSameTypeAndSameId_SkipsSelfPair()
    {
        List<InteractionAtomicPair> pairs = InteractionAtomicPairs.BuildPairs(
            ShortInteractionType.Substance,
            [IdA, IdB],
            ShortInteractionType.Substance,
            [IdA]);

        pairs.Should().Equal(new InteractionAtomicPair(ShortInteractionType.Substance, IdA, ShortInteractionType.Substance, IdB));
    }

    [Fact]
    public void BuildPairs_WhenSidesOverlap_CollapsesMirroredPairsToOne()
    {
        List<InteractionAtomicPair> pairs = InteractionAtomicPairs.BuildPairs(
            ShortInteractionType.Substance,
            [IdB, IdC],
            ShortInteractionType.Substance,
            [IdB, IdC]);

        pairs.Should().Equal(new InteractionAtomicPair(ShortInteractionType.Substance, IdB, ShortInteractionType.Substance, IdC));
    }

    [Fact]
    public void BuildPairs_WhenTypesDiffer_PutsSmallerTypeFirst()
    {
        List<InteractionAtomicPair> pairs = InteractionAtomicPairs.BuildPairs(
            ShortInteractionType.Food,
            [IdA],
            ShortInteractionType.Substance,
            [IdB]);

        pairs.Should().Equal(new InteractionAtomicPair(ShortInteractionType.Substance, IdB, ShortInteractionType.Food, IdA));
    }

    [Fact]
    public void BuildPairs_PreservesFirstSeenOrder()
    {
        List<InteractionAtomicPair> pairs = InteractionAtomicPairs.BuildPairs(
            ShortInteractionType.Substance,
            [IdC, IdA],
            ShortInteractionType.Food,
            [IdB]);

        pairs.Select(pair => pair.PartyIdA).Should().Equal(IdC, IdA);
    }

    [Fact]
    public void BuildPairs_WhenInputsAreLarge_ReturnsExactCountOfDistinctPairs()
    {
        Guid[] left = Enumerable.Range(0, 300).Select(_ => Guid.NewGuid()).ToArray();
        Guid[] right = Enumerable.Range(0, 300).Select(_ => Guid.NewGuid()).ToArray();

        List<InteractionAtomicPair> pairs = InteractionAtomicPairs.BuildPairs(
            ShortInteractionType.Substance,
            left,
            ShortInteractionType.Food,
            right);

        pairs.Should().HaveCount(90_000);
        pairs.Distinct().Should().HaveCount(90_000);
    }
}
