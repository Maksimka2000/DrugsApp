using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class ReportInfoSectionBuilderTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly RegularDrugIntakeReadModel MorningIntake = new(["08:00"], null, null, true);

    [Fact]
    public void Build_SplitsDrugsIntoTakenNowAndSelected_AndListsBothRolesOfOneForm()
    {
        InfoDrugSelection manual = CreateSelection("Apap", SelectedDrugSource.Manual, regularIntake: null);
        InfoDrugSelection regular = CreateSelection("Warfarin", SelectedDrugSource.UserRegular, MorningIntake);
        InfoDrugSelection both = CreateSelection("Amotaks", SelectedDrugSource.ManualAndUserRegular, MorningIntake);

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [manual, regular, both],
            includeUserData: true,
            regularSupplements: [],
            regularFoods: []);

        info.Drugs.Selected.Rows.Select(row => row.CommercialName).Should().Equal("Amotaks", "Apap");
        info.Drugs.Selected.Rows.Should().OnlyContain(row => row.RegularIntake == null);
        info.Drugs.TakenNow!.Rows.Select(row => row.CommercialName).Should().Equal("Amotaks", "Warfarin");
        info.Drugs.TakenNow.Rows.Should().OnlyContain(row => row.RegularIntake == MorningIntake);
        info.Drugs.Selected.Rows.First().ActiveSubstances.Should().ContainSingle()
            .Which.Name.Should().Be("Amotaks substance");
        info.Drugs.SelectedTwice!.SectionCode.Should().Be("info.drugs.selected_twice");
        info.Drugs.SelectedTwice.Rows.Should().ContainSingle()
            .Which.Should().Match<InfoDrugRowReadModel>(row =>
                row.CommercialName == "Amotaks" && row.RegularIntake == MorningIntake);
    }

    [Fact]
    public void Build_WithoutUserData_ListsOnlySelectedDrugs()
    {
        InfoDrugSelection publicSelection = CreateSelection("Apap", source: null, regularIntake: null);

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [publicSelection],
            includeUserData: false,
            regularSupplements: [],
            regularFoods: []);

        info.SectionCode.Should().Be("info");
        info.Drugs.TakenNow.Should().BeNull();
        info.Drugs.SelectedTwice.Should().BeNull();
        info.Drugs.Selected.RecordCount.Should().Be(1);
        info.Supplements!.TakenNow.Should().BeNull();
        info.Supplements.OwnSupplements.Should().BeNull();
        info.Supplements.SelectedTwice.Should().BeNull();
        info.Supplements.Selected.RecordCount.Should().Be(0);
        info.Foods!.TakenNow.Should().BeNull();
        info.Foods.Selected.RecordCount.Should().Be(0);
    }

    [Fact]
    public void Build_ListsCatalogSupplementsOwnSupplementsWithMembersAndFoods()
    {
        Supplement vitaminC = Supplement.Create("Vitamin C", ["Ascorbic acid"]);
        ClientRegularSupplement catalogRow = ClientRegularSupplement.Create(
            UserId,
            RegularSupplementSource.Database,
            vitaminC.Id,
            ownSupplementId: null,
            startDate: null,
            endDate: null,
            intakeTimes: [new TimeOnly(9, 0)],
            Today).Value;
        OwnSupplementMembershipBuilderTests.SetProperty(catalogRow, nameof(ClientRegularSupplement.Supplement), vitaminC);

        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        ClientRegularSupplement ownRow = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Night mix", magnesium, zinc);

        Food milk = Food.Create("Milk", ["Dairy"]).Value;
        ClientRegularFood foodRow = ClientRegularFood.Create(
            UserId,
            milk.Id,
            startDate: null,
            endDate: null,
            [new TimeOnly(7, 30)],
            Today).Value;
        OwnSupplementMembershipBuilderTests.SetProperty(foodRow, nameof(ClientRegularFood.Food), milk);

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [],
            includeUserData: true,
            regularSupplements: [catalogRow, ownRow],
            regularFoods: [foodRow]);

        InfoSupplementRowReadModel supplementRow = info.Supplements!.TakenNow!.Rows.Single();
        supplementRow.SupplementId.Should().Be(vitaminC.Id);
        supplementRow.DisplayLine.Should().Be("Vitamin C (Ascorbic acid)");
        supplementRow.RegularIntake.IntakeTimes.Should().Equal("09:00");

        InfoOwnSupplementRowReadModel ownSupplementRow = info.Supplements.OwnSupplements!.Rows.Single();
        ownSupplementRow.OwnSupplementName.Should().Be("Night mix");
        ownSupplementRow.MemberSupplements.Select(member => member.SupplementName).Should().Equal("Magnesium", "Zinc");
        ownSupplementRow.DisplayLine.Should().Be("Night mix: Magnesium, Zinc");

        InfoFoodRowReadModel infoFoodRow = info.Foods!.TakenNow!.Rows.Single();
        infoFoodRow.FoodName.Should().Be("Milk");
        infoFoodRow.DisplayLine.Should().Be("Milk (Dairy)");
        infoFoodRow.RegularIntake.IntakeTimes.Should().Equal("07:30");
    }

    [Fact]
    public void Build_ListsSelectedSupplementsAndFoods_AndMarksWhichAreAlreadyTakenRegularly()
    {
        Supplement zinc = Supplement.Create("Zinc", ["Zn"]);
        Supplement magnesium = Supplement.Create("Magnesium", aliases: null);
        Food grapefruit = Food.Create("Grapefruit", aliases: null).Value;
        ClientRegularSupplement ownRow = OwnSupplementMembershipBuilderTests.CreateOwnRegular("Night mix", magnesium);

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [],
            includeUserData: true,
            regularSupplements: [ownRow],
            regularFoods: [],
            selectedSupplements: [zinc, magnesium],
            selectedFoods: [grapefruit]);

        info.Supplements!.Selected.SectionCode.Should().Be("info.supplements.selected");
        info.Supplements.Selected.Rows.Select(row => (row.Name, row.IsAlsoTakenNow))
            .Should().Equal(("Magnesium", true), ("Zinc", false));

        // only the supplement that is also taken inside an own supplement is selected twice
        InfoSupplementSelectedTwiceRowReadModel twice = info.Supplements.SelectedTwice!.Rows.Should().ContainSingle().Subject;
        twice.SupplementId.Should().Be(magnesium.Id);
        twice.IsTakenDirectly.Should().BeFalse();
        twice.RegularIntake.Should().BeNull();
        twice.OwnSupplements.Should().ContainSingle()
            .Which.OwnSupplementName.Should().Be("Night mix");

        info.Foods!.Selected.Rows.Should().ContainSingle()
            .Which.Should().Match<InfoSelectedItemRowReadModel>(row =>
                row.Id == grapefruit.Id && row.IsAlsoTakenNow == false);
    }

    [Fact]
    public void Build_WithoutUserData_ListsSelectedSupplementsAndFoodsWithoutTakenNowFlags()
    {
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        Food grapefruit = Food.Create("Grapefruit", aliases: null).Value;

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [],
            includeUserData: false,
            regularSupplements: [],
            regularFoods: [],
            selectedSupplements: [zinc],
            selectedFoods: [grapefruit],
            audience: ReportAudience.Public);

        info.Supplements!.Selected.Rows.Should().ContainSingle().Which.IsAlsoTakenNow.Should().BeNull();
        info.Supplements.SelectedTwice.Should().BeNull();
        info.Foods!.Selected.Rows.Should().ContainSingle().Which.IsAlsoTakenNow.Should().BeNull();
    }

    [Fact]
    public void Build_ForProvider_UsesSelectedForPrescriptionHeadlines()
    {
        Supplement zinc = Supplement.Create("Zinc", aliases: null);
        Food grapefruit = Food.Create("Grapefruit", aliases: null).Value;
        InfoDrugSelection manual = CreateSelection("Apap", SelectedDrugSource.Manual, regularIntake: null);

        ReportInfoSectionReadModel info = ReportInfoSectionBuilder.Build(
            [manual],
            includeUserData: true,
            regularSupplements: [],
            regularFoods: [],
            selectedSupplements: [zinc],
            selectedFoods: [grapefruit],
            audience: ReportAudience.Provider);

        info.Drugs.Selected.HeadlineText.Should().Be("DRUGS SELECTED FOR PRESCRIPTION");
        info.Supplements!.Selected.HeadlineText.Should().Be("SUPPLEMENTS SELECTED FOR PRESCRIPTION");
        info.Foods!.Selected.HeadlineText.Should().Be("FOODS SELECTED FOR PRESCRIPTION");
    }

    [Fact]
    public void BuildOwnSupplementDisplayLine_NamesTheOwnSupplement()
    {
        string displayLine = DrugInteractionToolReportMapping.BuildOwnSupplementDisplayLine(
            "Amoxicillin",
            "Probiotics (lactobacillus)",
            "Gut mix",
            "Mineral binding reduces drug bioavailability.",
            stomachText: null,
            "Moderate");

        displayLine.Should().Be(
            "Amoxicillin => Probiotics (lactobacillus) in own supplement (Gut mix): Mineral binding reduces drug bioavailability. Moderate");
    }

    private static InfoDrugSelection CreateSelection(
        string commercialName,
        SelectedDrugSource? source,
        RegularDrugIntakeReadModel? regularIntake)
    {
        ActiveSubstance substance = ActiveSubstance.Create($"{commercialName} substance");
        Drug drug = Drug.Create(commercialName, "Ludzki", DrugType.Standard, [substance], drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        OwnSupplementMembershipBuilderTests.SetProperty(form, nameof(DrugForm.Drug), drug);
        drug.AddDrugForm(form);

        return new InfoDrugSelection(drug, form, source, regularIntake);
    }
}
