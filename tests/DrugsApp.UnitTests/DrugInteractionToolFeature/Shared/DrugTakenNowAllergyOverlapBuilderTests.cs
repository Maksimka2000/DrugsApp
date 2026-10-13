using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugTakenNowAllergyOverlapBuilderTests
{
    private static readonly DateOnly Today = new(2026, 9, 28);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void BuildRows_WhenSameCatalogDrugDifferentForms_EmitsKindZeroOnly()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");
        (Drug drug, DrugForm takenForm, DrugForm allergyForm) = CreateDrugWithTwoForms(
            "Augmentin",
            substance);

        ClientRegularDrug regular = CreateActiveRegular(drug, takenForm);
        ClientDrugAllergy allergy = CreateDatabaseAllergy(allergyForm, "Rash");

        IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> rows =
            DrugTakenNowAllergyOverlapBuilder.BuildRows([regular], [allergy]);

        rows.Should().ContainSingle();
        DrugTakenNowAllergyOverlapRowReadModel row = rows.Single();
        row.OverlapKind.Should().Be(DrugTakenNowAllergyOverlapKind.SameDrugName);
        row.RegularDrugId.Should().Be(drug.Id);
        row.RegularDrugFormId.Should().Be(takenForm.Id);
        row.AllergyDrugId.Should().Be(drug.Id);
        row.AllergyDrugFormId.Should().Be(allergyForm.Id);
        row.ActiveSubstanceId.Should().BeNull();
        row.Title.Should().Be(DrugInteractionToolHeadlines.DrugTakenNowOverlapSameDrugName);
    }

    [Fact]
    public void BuildRows_WhenDifferentDrugsShareSubstances_EmitsOneKindOneRowPerSubstance()
    {
        ActiveSubstance metformin = ActiveSubstance.Create("Metformin");
        ActiveSubstance sitagliptin = ActiveSubstance.Create("Sitagliptin");
        (Drug takenDrug, DrugForm takenForm) = CreateDrug("Glucophage", metformin, sitagliptin);
        (Drug allergyDrug, DrugForm allergyForm) = CreateDrug("Janumet", metformin, sitagliptin);

        ClientRegularDrug regular = CreateActiveRegular(takenDrug, takenForm);
        ClientDrugAllergy allergy = CreateDatabaseAllergy(allergyForm, "Rash");

        IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> rows =
            DrugTakenNowAllergyOverlapBuilder.BuildRows([regular], [allergy]);

        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(row => row.OverlapKind == DrugTakenNowAllergyOverlapKind.SharedActiveSubstance);
        rows.Select(row => row.ActiveSubstanceName).Should().Equal("Metformin", "Sitagliptin");
        rows.Should().OnlyContain(row => row.RegularDrugId == takenDrug.Id && row.AllergyDrugId == allergyDrug.Id);
    }

    [Fact]
    public void BuildRows_WhenNoOverlap_ReturnsEmptySubsection()
    {
        (Drug takenDrug, DrugForm takenForm) = CreateDrug("Ibuprofen", ActiveSubstance.Create("Ibuprofen"));
        (Drug allergyDrug, DrugForm allergyForm) = CreateDrug("Paracetamol", ActiveSubstance.Create("Paracetamol"));

        ClientRegularDrug regular = CreateActiveRegular(takenDrug, takenForm);
        ClientDrugAllergy allergy = CreateDatabaseAllergy(allergyForm, "Rash");
        ClientDrugAllergy customAllergy = ClientDrugAllergy.CreateCustom(
            UserId,
            "Bactrim",
            "Sulfamethoxazole",
            "Tablet",
            "800 mg",
            "Note").Value;

        ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> subsection =
            DrugTakenNowAllergyOverlapBuilder.BuildSubsection(
                [regular],
                [allergy, customAllergy]);

        subsection.SectionCode.Should().Be("allergies.drug.taken_now_overlap");
        subsection.RecordCount.Should().Be(0);
        subsection.Rows.Should().BeEmpty();
        allergyDrug.Id.Should().NotBe(takenDrug.Id);
    }

    [Fact]
    public void BuildRows_WhenRegularIsScheduled_SkipsRow()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");
        (Drug drug, DrugForm form) = CreateDrug("Augmentin", substance);
        ClientRegularDrug scheduled = ClientRegularDrug.Create(
            UserId,
            drug.Id,
            form.Id,
            startDate: Today.AddDays(5),
            endDate: null,
            intakeTimes: null,
            Today).Value;
        scheduled.AttachCatalogReferences(drug, form);
        ClientDrugAllergy allergy = CreateDatabaseAllergy(form, "Rash");

        IReadOnlyCollection<DrugTakenNowAllergyOverlapRowReadModel> rows =
            DrugTakenNowAllergyOverlapBuilder.BuildRows([scheduled], [allergy]);

        rows.Should().BeEmpty();
    }

    [Fact]
    public void BuildPlanTakingSubsection_WhenPlannedDrugSharesSubstance_EmitsRowWithPlanSectionCode()
    {
        ActiveSubstance metformin = ActiveSubstance.Create("Metformin");
        (Drug plannedDrug, DrugForm plannedForm) = CreateDrug("Glucophage", metformin);
        (Drug allergyDrug, DrugForm allergyForm) = CreateDrug("Janumet", metformin);
        ClientDrugAllergy allergy = CreateDatabaseAllergy(allergyForm, "Rash");

        ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> subsection =
            DrugTakenNowAllergyOverlapBuilder.BuildPlanTakingSubsection(
                [new DrugFormIntake(plannedDrug, plannedForm)],
                [allergy]);

        subsection.SectionCode.Should().Be("allergies.drug.plan_taking_overlap");
        subsection.HeadlineText.Should().Be(DrugInteractionToolHeadlines.DrugAllergiesPlanTakingOverlap);
        subsection.RecordCount.Should().Be(1);
        DrugTakenNowAllergyOverlapRowReadModel row = subsection.Rows.Single();
        row.OverlapKind.Should().Be(DrugTakenNowAllergyOverlapKind.SharedActiveSubstance);
        row.RegularDrugId.Should().Be(plannedDrug.Id);
        row.AllergyDrugId.Should().Be(allergyDrug.Id);
        row.ActiveSubstanceName.Should().Be("Metformin");
    }

    [Fact]
    public void BuildPlanTakingSubsection_WhenNoPlannedDrugs_ReturnsEmptySubsection()
    {
        (Drug _, DrugForm allergyForm) = CreateDrug("Janumet", ActiveSubstance.Create("Metformin"));
        ClientDrugAllergy allergy = CreateDatabaseAllergy(allergyForm, "Rash");

        ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> subsection =
            DrugTakenNowAllergyOverlapBuilder.BuildPlanTakingSubsection([], [allergy]);

        subsection.SectionCode.Should().Be("allergies.drug.plan_taking_overlap");
        subsection.RecordCount.Should().Be(0);
        subsection.Rows.Should().BeEmpty();
    }

    private static ClientRegularDrug CreateActiveRegular(Drug drug, DrugForm form)
    {
        ClientRegularDrug regular = ClientRegularDrug.Create(
            UserId,
            drug.Id,
            form.Id,
            startDate: Today,
            endDate: null,
            intakeTimes: null,
            Today).Value;
        regular.AttachCatalogReferences(drug, form);
        return regular;
    }

    private static ClientDrugAllergy CreateDatabaseAllergy(DrugForm form, string note)
    {
        ClientDrugAllergy allergy = ClientDrugAllergy.CreateDatabase(UserId, form.Id, note).Value;
        allergy.AttachDrugForm(form);
        return allergy;
    }

    private static (Drug Drug, DrugForm Form) CreateDrug(string name, params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        AttachOwningDrug(form, drug);
        drug.AddDrugForm(form);
        return (drug, form);
    }

    private static (Drug Drug, DrugForm TakenForm, DrugForm AllergyForm) CreateDrugWithTwoForms(
        string name,
        params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm takenForm = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        DrugForm allergyForm = DrugForm.Create(drug.Id, productIdentifier: 2, dosage: "20 mg");
        AttachOwningDrug(takenForm, drug);
        AttachOwningDrug(allergyForm, drug);
        drug.AddDrugForm(takenForm);
        drug.AddDrugForm(allergyForm);
        return (drug, takenForm, allergyForm);
    }

    private static void AttachOwningDrug(DrugForm form, Drug drug)
    {
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
    }
}
