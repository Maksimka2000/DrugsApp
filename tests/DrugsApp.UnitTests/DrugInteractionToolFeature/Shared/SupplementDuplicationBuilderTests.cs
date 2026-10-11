using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class SupplementDuplicationBuilderTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void BuildRows_WhenCatalogSupplementIsAlsoInsideOwnSupplement_ReturnsRowWithBothSources()
    {
        Supplement ashwagandha = Supplement.Create("Ashwagandha", ["Withania somnifera"]);
        Supplement iron = Supplement.Create("Iron", aliases: null);
        ClientRegularSupplement catalogRow = CreateCatalogRegular(ashwagandha);
        ClientRegularSupplement ownRow = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Test", ashwagandha, iron);

        List<SupplementDuplicationRowReadModel> rows = SupplementDuplicationBuilder.BuildRows([catalogRow, ownRow]);

        SupplementDuplicationRowReadModel row = rows.Should().ContainSingle().Subject;
        row.SupplementId.Should().Be(ashwagandha.Id);
        row.SupplementSummaryText.Should().Be("Ashwagandha (Withania somnifera)");
        row.DuplicationKind.Should().Be(DuplicationKind.Supplement);
        row.AffectedSupplements.Should().ContainSingle()
            .Which.Should().Be(new DuplicationSupplementSummaryReadModel(
                ashwagandha.Id,
                "Ashwagandha (Withania somnifera)",
                DuplicationDrugSource.TakingNow));
        row.AffectedOwnSupplements.Should().ContainSingle()
            .Which.Should().Be(new DuplicationOwnSupplementSummaryReadModel(
                ownRow.OwnSupplement!.Id,
                "Test | Contains: Ashwagandha, Iron",
                DuplicationDrugSource.TakingNow));
    }

    [Fact]
    public void BuildRows_WhenSupplementIsInTwoOwnSupplements_ReturnsRowWithoutCatalogSource()
    {
        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        ClientRegularSupplement night = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Night mix", magnesium);
        ClientRegularSupplement morning = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Morning mix", magnesium);

        List<SupplementDuplicationRowReadModel> rows = SupplementDuplicationBuilder.BuildRows([night, morning]);

        SupplementDuplicationRowReadModel row = rows.Should().ContainSingle().Subject;
        row.AffectedSupplements.Should().BeEmpty();
        row.AffectedOwnSupplements.Select(own => own.SupplementSummaryText)
            .Should().Equal("Morning mix | Contains: Magnesium", "Night mix | Contains: Magnesium");
    }

    [Fact]
    public void BuildRows_WhenEverySupplementHasOneSource_ReturnsNoRows()
    {
        Supplement vitaminC = Supplement.Create("Vitamin C", aliases: null);
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        ClientRegularSupplement catalogRow = CreateCatalogRegular(vitaminC);
        ClientRegularSupplement ownRow = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Night mix", zinc);
        ClientRegularSupplement sameOwnAgain = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Night mix", zinc);
        OwnSupplementMembershipBuilderTests.SetProperty(
            sameOwnAgain,
            nameof(ClientRegularSupplement.OwnSupplement),
            ownRow.OwnSupplement);

        List<SupplementDuplicationRowReadModel> rows = SupplementDuplicationBuilder.BuildRows(
            [catalogRow, ownRow, sameOwnAgain]);

        rows.Should().BeEmpty();
    }

    [Fact]
    public void BuildRows_WhenSelectedSupplementIsAlsoTakenAndInsideOwnSupplement_ListsAllThreeSources()
    {
        Supplement ashwagandha = Supplement.Create("Ashwagandha", aliases: null);
        Supplement iron = Supplement.Create("Iron", aliases: null);
        ClientRegularSupplement catalogRow = CreateCatalogRegular(ashwagandha);
        ClientRegularSupplement ownRow = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Test", ashwagandha, iron);

        List<SupplementDuplicationRowReadModel> rows = SupplementDuplicationBuilder.BuildRows(
            [catalogRow, ownRow],
            [ashwagandha]);

        SupplementDuplicationRowReadModel row = rows.Should().ContainSingle().Subject;
        row.AffectedSupplements.Select(supplement => supplement.Source)
            .Should().Equal(DuplicationDrugSource.TakingNow, DuplicationDrugSource.Selected);
        row.AffectedOwnSupplements.Should().ContainSingle();
        row.DisplayLine.Should().Contain("IS SELECTED AND ALREADY TAKEN REGULARLY");
    }

    [Fact]
    public void BuildRows_WhenSelectedSupplementIsNotTakenAnywhere_ReturnsNoRows()
    {
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        Supplement iron = Supplement.Create("Iron", aliases: null);
        ClientRegularSupplement catalogRow = CreateCatalogRegular(iron);

        List<SupplementDuplicationRowReadModel> rows = SupplementDuplicationBuilder.BuildRows([catalogRow], [zinc]);

        rows.Should().BeEmpty();
    }

    private static ClientRegularSupplement CreateCatalogRegular(Supplement supplement)
    {
        ClientRegularSupplement row = ClientRegularSupplement.Create(
            UserId,
            RegularSupplementSource.Database,
            supplement.Id,
            ownSupplementId: null,
            startDate: null,
            endDate: null,
            intakeTimes: [new TimeOnly(9, 0)],
            Today).Value;

        OwnSupplementMembershipBuilderTests.SetProperty(row, nameof(ClientRegularSupplement.Supplement), supplement);
        return row;
    }
}
