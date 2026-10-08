using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugDuplicationBuilderTests
{
    [Fact]
    public void Build_WhenDifferentDrugsShareASubstance_ReportsOnlyAnExactDuplicate()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        DuplicationSelection nurofen = CreateSelection("Nurofen", SelectedDrugSource.Manual, ibuprofen);
        DuplicationSelection ibum = CreateSelection("Ibum", SelectedDrugSource.Manual, ibuprofen);

        DrugDuplicationSectionReadModel section = Build([nurofen, ibum], [], ibuprofen);

        ExactDuplicationRowReadModel row = section.ExactActiveSubstanceDuplicates.Rows.Single();
        row.ActiveSubstanceName.Should().Be("Ibuprofen");
        row.AffectedDrugs.Select(drug => drug.DrugFormId)
            .Should().BeEquivalentTo([nurofen.Form.Id, ibum.Form.Id]);
        section.DrugDuplicates.Rows.Should().BeEmpty();
        section.DrugFormDuplicates.Rows.Should().BeEmpty();
        section.ActiveSubstanceGroupDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenTwoFormsOfTheSameDrugAreSelected_ReportsADrugDuplicateAndNoExactDuplicate()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        (Drug drug, DrugForm form200, DrugForm form400) = CreateDrugWithTwoForms("Nurofen", ibuprofen);

        DrugDuplicationSectionReadModel section = Build(
            [
                new DuplicationSelection(drug, form200, SelectedDrugSource.Manual),
                new DuplicationSelection(drug, form400, SelectedDrugSource.Manual)
            ],
            [],
            ibuprofen);

        DrugDuplicationRowReadModel row = section.DrugDuplicates.Rows.Single();
        row.DrugId.Should().Be(drug.Id);
        row.DrugName.Should().Be("Nurofen");
        row.DuplicationKind.Should().Be(DuplicationKind.Drug);
        row.AffectedDrugs.Select(affected => affected.DrugFormId)
            .Should().BeEquivalentTo([form200.Id, form400.Id]);
        section.ExactActiveSubstanceDuplicates.Rows.Should().BeEmpty();
        section.DrugFormDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenSelectedFormIsAlreadyTakenRegularly_ReportsADrugFormDuplicateOnly()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        DuplicationSelection nurofen = CreateSelection(
            "Nurofen",
            SelectedDrugSource.ManualAndUserRegular,
            ibuprofen);

        DrugDuplicationSectionReadModel section = Build([nurofen], [], ibuprofen);

        DrugFormDuplicationRowReadModel row = section.DrugFormDuplicates.Rows.Single();
        row.DrugFormId.Should().Be(nurofen.Form.Id);
        row.DuplicationKind.Should().Be(DuplicationKind.DrugForm);
        row.AffectedDrugs.Select(affected => affected.Source)
            .Should().BeEquivalentTo([DuplicationDrugSource.TakingNow, DuplicationDrugSource.Selected]);
        section.ExactActiveSubstanceDuplicates.Rows.Should().BeEmpty();
        section.DrugDuplicates.Rows.Should().BeEmpty();
        section.ActiveSubstanceGroupDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenTakenFormAndASiblingFormAreSelected_ReportsBothTheFormAndTheDrugDuplicate()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        (Drug drug, DrugForm form200, DrugForm form400) = CreateDrugWithTwoForms("Nurofen", ibuprofen);

        DrugDuplicationSectionReadModel section = Build(
            [
                new DuplicationSelection(drug, form200, SelectedDrugSource.ManualAndUserRegular),
                new DuplicationSelection(drug, form400, SelectedDrugSource.Manual)
            ],
            [],
            ibuprofen);

        section.DrugFormDuplicates.Rows.Single().DrugFormId.Should().Be(form200.Id);
        section.DrugDuplicates.Rows.Single().AffectedDrugs.Should().HaveCount(3);
        section.ExactActiveSubstanceDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenADrugWithTwoFormsSharesASubstanceWithAnotherDrug_ReportsAllThreeKindsWhereTheyApply()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        (Drug drug, DrugForm form200, DrugForm form400) = CreateDrugWithTwoForms("Nurofen", ibuprofen);
        DuplicationSelection ibum = CreateSelection("Ibum", SelectedDrugSource.Manual, ibuprofen);

        DrugDuplicationSectionReadModel section = Build(
            [
                new DuplicationSelection(drug, form200, SelectedDrugSource.Manual),
                new DuplicationSelection(drug, form400, SelectedDrugSource.Manual),
                ibum
            ],
            [],
            ibuprofen);

        section.ExactActiveSubstanceDuplicates.Rows.Single().AffectedDrugs.Should().HaveCount(3);
        section.DrugDuplicates.Rows.Single().DrugId.Should().Be(drug.Id);
        section.DrugFormDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenDifferentDrugsContainDifferentMembersOfAGroup_ReportsAGroupDuplicate()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        ActiveSubstance naproxen = ActiveSubstance.Create("Naproxen");
        ActiveSubstanceGroup nsaids = ActiveSubstanceGroup.Create("NSAIDs", [ibuprofen, naproxen]);
        DuplicationSelection nurofen = CreateSelection("Nurofen", SelectedDrugSource.Manual, ibuprofen);
        DuplicationSelection naprosyn = CreateSelection("Naprosyn", SelectedDrugSource.Manual, naproxen);

        DrugDuplicationSectionReadModel section = Build([nurofen, naprosyn], [nsaids], ibuprofen, naproxen);

        GroupDuplicationRowReadModel row = section.ActiveSubstanceGroupDuplicates.Rows.Single();
        row.GroupName.Should().Be("NSAIDs");
        row.AffectedActiveSubstances.Select(substance => substance.AffectedSubstanceName)
            .Should().Equal("Ibuprofen", "Naproxen");
        section.ExactActiveSubstanceDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenOneDrugContainsSeveralMembersOfAGroup_DoesNotReportAGroupDuplicate()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        ActiveSubstance naproxen = ActiveSubstance.Create("Naproxen");
        ActiveSubstanceGroup nsaids = ActiveSubstanceGroup.Create("NSAIDs", [ibuprofen, naproxen]);
        DuplicationSelection combination = CreateSelection(
            "Combination",
            SelectedDrugSource.ManualAndUserRegular,
            ibuprofen,
            naproxen);

        DrugDuplicationSectionReadModel section = Build([combination], [nsaids], ibuprofen, naproxen);

        section.ActiveSubstanceGroupDuplicates.Rows.Should().BeEmpty();
        section.ExactActiveSubstanceDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenGroupOverlapIsFullyExplainedByAnExactDuplicate_SkipsTheGroupRow()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        ActiveSubstance naproxen = ActiveSubstance.Create("Naproxen");
        ActiveSubstanceGroup nsaids = ActiveSubstanceGroup.Create("NSAIDs", [ibuprofen, naproxen]);
        DuplicationSelection nurofen = CreateSelection("Nurofen", SelectedDrugSource.Manual, ibuprofen);
        DuplicationSelection ibum = CreateSelection("Ibum", SelectedDrugSource.Manual, ibuprofen);

        DrugDuplicationSectionReadModel section = Build([nurofen, ibum], [nsaids], ibuprofen, naproxen);

        section.ExactActiveSubstanceDuplicates.Rows.Should().ContainSingle();
        section.ActiveSubstanceGroupDuplicates.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenNothingIsDuplicated_ReturnsFourEmptySubsections()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        ActiveSubstance warfarin = ActiveSubstance.Create("Warfarin");

        DrugDuplicationSectionReadModel section = Build(
            [
                CreateSelection("Nurofen", SelectedDrugSource.Manual, ibuprofen),
                CreateSelection("Warfin", SelectedDrugSource.UserRegular, warfarin)
            ],
            [],
            ibuprofen,
            warfarin);

        section.ExactActiveSubstanceDuplicates.RecordCount.Should().Be(0);
        section.ActiveSubstanceGroupDuplicates.RecordCount.Should().Be(0);
        section.DrugDuplicates.RecordCount.Should().Be(0);
        section.DrugFormDuplicates.RecordCount.Should().Be(0);
        section.DrugDuplicates.SectionCode.Should().Be("drug_duplication.drug");
        section.DrugFormDuplicates.SectionCode.Should().Be("drug_duplication.drug_form");
    }

    [Fact]
    public void HasMultipleDrugs_IsTrueOnlyForTwoOrMoreDistinctDrugs()
    {
        ActiveSubstance ibuprofen = ActiveSubstance.Create("Ibuprofen");
        (Drug drug, DrugForm form200, DrugForm form400) = CreateDrugWithTwoForms("Nurofen", ibuprofen);
        DuplicationSelection other = CreateSelection("Ibum", SelectedDrugSource.Manual, ibuprofen);

        DrugDuplicationBuilder.HasMultipleDrugs([
                new DuplicationSelection(drug, form200, null),
                new DuplicationSelection(drug, form400, null)
            ]).Should().BeFalse();
        DrugDuplicationBuilder.HasMultipleDrugs([new DuplicationSelection(drug, form200, null), other])
            .Should().BeTrue();
    }

    private static DrugDuplicationSectionReadModel Build(
        IReadOnlyList<DuplicationSelection> selections,
        IReadOnlyCollection<ActiveSubstanceGroup> groups,
        params ActiveSubstance[] substances) =>
        DrugDuplicationBuilder.Build(
            selections,
            substances.ToDictionary(substance => substance.Id, substance => substance.ActiveSubstanceName),
            groups);

    private static DuplicationSelection CreateSelection(
        string name,
        SelectedDrugSource source,
        params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        AttachOwningDrug(form, drug);
        drug.AddDrugForm(form);
        return new DuplicationSelection(drug, form, source);
    }

    private static (Drug Drug, DrugForm Form200, DrugForm Form400) CreateDrugWithTwoForms(
        string name,
        params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm form200 = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "200 mg");
        DrugForm form400 = DrugForm.Create(drug.Id, productIdentifier: 2, dosage: "400 mg");
        AttachOwningDrug(form200, drug);
        AttachOwningDrug(form400, drug);
        drug.AddDrugForm(form200);
        drug.AddDrugForm(form400);
        return (drug, form200, form400);
    }

    private static void AttachOwningDrug(DrugForm form, Drug drug)
    {
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
    }
}
