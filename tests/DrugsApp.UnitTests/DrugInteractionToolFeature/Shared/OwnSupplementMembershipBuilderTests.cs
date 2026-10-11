using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class OwnSupplementMembershipBuilderTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void BuildSourcesBySupplementId_MapsEveryMemberToItsOwnSupplement()
    {
        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        ClientRegularSupplement regular = CreateOwnRegular("Night mix", magnesium, zinc);

        Dictionary<Guid, IReadOnlyList<OwnSupplementSource>> sources =
            OwnSupplementMembershipBuilder.BuildSourcesBySupplementId([regular]);

        sources.Keys.Should().BeEquivalentTo([magnesium.Id, zinc.Id]);
        sources[magnesium.Id].Should().ContainSingle()
            .Which.Should().Be(new OwnSupplementSource(regular.OwnSupplement!.Id, "Night mix"));
    }

    [Fact]
    public void BuildSourcesBySupplementId_WhenMemberIsInTwoOwnSupplements_ListsBothOrderedByName()
    {
        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        ClientRegularSupplement night = CreateOwnRegular("Night mix", magnesium);
        ClientRegularSupplement morning = CreateOwnRegular("Morning mix", magnesium);

        Dictionary<Guid, IReadOnlyList<OwnSupplementSource>> sources =
            OwnSupplementMembershipBuilder.BuildSourcesBySupplementId([night, morning]);

        sources[magnesium.Id].Select(source => source.OwnSupplementName)
            .Should().Equal("Morning mix", "Night mix");
    }

    [Fact]
    public void BuildSourcesBySupplementId_IgnoresCatalogRowsAndRepeatedOwnSupplements()
    {
        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        ClientRegularSupplement own = CreateOwnRegular("Night mix", magnesium);
        ClientRegularSupplement sameOwnAgain = CreateOwnRegular("Night mix", magnesium);
        SetProperty(sameOwnAgain, nameof(ClientRegularSupplement.OwnSupplement), own.OwnSupplement);
        ClientRegularSupplement catalog = ClientRegularSupplement.Create(
            UserId,
            RegularSupplementSource.Database,
            magnesium.Id,
            ownSupplementId: null,
            startDate: null,
            endDate: null,
            intakeTimes: null,
            Today).Value;

        Dictionary<Guid, IReadOnlyList<OwnSupplementSource>> sources =
            OwnSupplementMembershipBuilder.BuildSourcesBySupplementId([own, sameOwnAgain, catalog]);

        sources[magnesium.Id].Should().ContainSingle();
    }

    internal static ClientRegularSupplement CreateOwnRegular(string name, params Supplement[] members)
    {
        ClientOwnSupplement ownSupplement = ClientOwnSupplement.Create(
            UserId,
            name,
            members.Select(member => member.Id).ToList()).Value;

        foreach (ClientOwnSupplementItem item in ownSupplement.Items)
        {
            SetProperty(item, nameof(ClientOwnSupplementItem.Supplement), members.Single(member => member.Id == item.SupplementId));
        }

        ClientRegularSupplement regular = ClientRegularSupplement.Create(
            UserId,
            RegularSupplementSource.Own,
            supplementId: null,
            ownSupplement.Id,
            startDate: null,
            endDate: null,
            intakeTimes: [new TimeOnly(8, 0)],
            Today).Value;

        SetProperty(regular, nameof(ClientRegularSupplement.OwnSupplement), ownSupplement);
        return regular;
    }

    internal static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }
}
