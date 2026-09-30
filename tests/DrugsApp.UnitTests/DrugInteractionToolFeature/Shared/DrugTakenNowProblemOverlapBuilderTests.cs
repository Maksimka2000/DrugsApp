using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugTakenNowProblemOverlapBuilderTests
{
    private static readonly DateOnly Today = new(2026, 9, 28);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void BuildRows_WhenSameCatalogDrugDifferentForms_EmitsKindZeroOnly()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");
        (Drug drug, DrugForm takenForm, DrugForm problemForm) = CreateDrugWithTwoForms(
            "Augmentin",
            substance);

        ClientRegularDrug regular = CreateActiveRegular(drug, takenForm);
        DrugProblem problem = CreateDatabaseProblem(problemForm, "Stomach pain");

        IReadOnlyCollection<DrugTakenNowProblemOverlapRowReadModel> rows =
            DrugTakenNowProblemOverlapBuilder.BuildRows([regular], [problem]);

        rows.Should().ContainSingle();
        DrugTakenNowProblemOverlapRowReadModel row = rows.Single();
        row.OverlapKind.Should().Be(DrugTakenNowAllergyOverlapKind.SameDrugName);
        row.RegularDrugId.Should().Be(drug.Id);
        row.RegularDrugFormId.Should().Be(takenForm.Id);
        row.ProblemDrugId.Should().Be(drug.Id);
        row.ProblemDrugFormId.Should().Be(problemForm.Id);
        row.ActiveSubstanceId.Should().BeNull();
        row.Title.Should().Be(DrugInteractionToolHeadlines.DrugTakenNowOverlapSameDrugName);
    }

    [Fact]
    public void BuildRows_WhenDifferentDrugsShareSubstances_EmitsOneKindOneRowPerSubstance()
    {
        ActiveSubstance metformin = ActiveSubstance.Create("Metformin");
        ActiveSubstance sitagliptin = ActiveSubstance.Create("Sitagliptin");
        (Drug takenDrug, DrugForm takenForm) = CreateDrug("Glucophage", metformin, sitagliptin);
        (Drug problemDrug, DrugForm problemForm) = CreateDrug("Janumet", metformin, sitagliptin);

        ClientRegularDrug regular = CreateActiveRegular(takenDrug, takenForm);
        DrugProblem problem = CreateDatabaseProblem(problemForm, "Nausea");

        IReadOnlyCollection<DrugTakenNowProblemOverlapRowReadModel> rows =
            DrugTakenNowProblemOverlapBuilder.BuildRows([regular], [problem]);

        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(row => row.OverlapKind == DrugTakenNowAllergyOverlapKind.SharedActiveSubstance);
        rows.Select(row => row.ActiveSubstanceName).Should().Equal("Metformin", "Sitagliptin");
        rows.Should().OnlyContain(row => row.RegularDrugId == takenDrug.Id && row.ProblemDrugId == problemDrug.Id);
    }

    [Fact]
    public void BuildRows_WhenNoOverlap_ReturnsEmptySubsection()
    {
        (Drug takenDrug, DrugForm takenForm) = CreateDrug("Ibuprofen", ActiveSubstance.Create("Ibuprofen"));
        (Drug problemDrug, DrugForm problemForm) = CreateDrug("Paracetamol", ActiveSubstance.Create("Paracetamol"));

        ClientRegularDrug regular = CreateActiveRegular(takenDrug, takenForm);
        DrugProblem problem = CreateDatabaseProblem(problemForm, "Liver concern");
        DrugProblem customProblem = DrugProblem.CreateCustom(
            UserId,
            "Bactrim",
            "Sulfamethoxazole",
            "Tablet",
            "800 mg",
            "Note").Value;

        ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> subsection =
            DrugTakenNowProblemOverlapBuilder.BuildSubsection(
                [regular],
                [problem, customProblem]);

        subsection.SectionCode.Should().Be("problematic_drugs.taken_now_overlap");
        subsection.RecordCount.Should().Be(0);
        subsection.Rows.Should().BeEmpty();
        problemDrug.Id.Should().NotBe(takenDrug.Id);
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
        DrugProblem problem = CreateDatabaseProblem(form, "Stomach pain");

        IReadOnlyCollection<DrugTakenNowProblemOverlapRowReadModel> rows =
            DrugTakenNowProblemOverlapBuilder.BuildRows([scheduled], [problem]);

        rows.Should().BeEmpty();
    }

    [Fact]
    public void BuildPlanTakingSubsection_WhenPlannedDrugSharesSubstance_EmitsRowWithPlanSectionCode()
    {
        ActiveSubstance metformin = ActiveSubstance.Create("Metformin");
        (Drug plannedDrug, DrugForm plannedForm) = CreateDrug("Glucophage", metformin);
        (Drug problemDrug, DrugForm problemForm) = CreateDrug("Janumet", metformin);
        DrugProblem problem = CreateDatabaseProblem(problemForm, "Rash");

        ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> subsection =
            DrugTakenNowProblemOverlapBuilder.BuildPlanTakingSubsection(
                [new DrugFormIntake(plannedDrug, plannedForm)],
                [problem]);

        subsection.SectionCode.Should().Be("problematic_drugs.plan_taking_overlap");
        subsection.HeadlineText.Should().Be(DrugInteractionToolHeadlines.ProblematicDrugsPlanTakingOverlap);
        subsection.RecordCount.Should().Be(1);
        DrugTakenNowProblemOverlapRowReadModel row = subsection.Rows.Single();
        row.OverlapKind.Should().Be(DrugTakenNowAllergyOverlapKind.SharedActiveSubstance);
        row.RegularDrugId.Should().Be(plannedDrug.Id);
        row.ProblemDrugId.Should().Be(problemDrug.Id);
        row.ActiveSubstanceName.Should().Be("Metformin");
    }

    [Fact]
    public void BuildPlanTakingSubsection_WhenNoPlannedDrugs_ReturnsEmptySubsection()
    {
        (Drug _, DrugForm problemForm) = CreateDrug("Janumet", ActiveSubstance.Create("Metformin"));
        DrugProblem problem = CreateDatabaseProblem(problemForm, "Rash");

        ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> subsection =
            DrugTakenNowProblemOverlapBuilder.BuildPlanTakingSubsection([], [problem]);

        subsection.SectionCode.Should().Be("problematic_drugs.plan_taking_overlap");
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

    private static DrugProblem CreateDatabaseProblem(DrugForm form, string note)
    {
        DrugProblem problem = DrugProblem.CreateDatabase(UserId, form.Id, note).Value;
        problem.AttachDrugForm(form);
        return problem;
    }

    private static (Drug Drug, DrugForm Form) CreateDrug(string name, params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        AttachOwningDrug(form, drug);
        drug.AddDrugForm(form);
        return (drug, form);
    }

    private static (Drug Drug, DrugForm TakenForm, DrugForm ProblemForm) CreateDrugWithTwoForms(
        string name,
        params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm takenForm = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        DrugForm problemForm = DrugForm.Create(drug.Id, productIdentifier: 2, dosage: "20 mg");
        AttachOwningDrug(takenForm, drug);
        AttachOwningDrug(problemForm, drug);
        drug.AddDrugForm(takenForm);
        drug.AddDrugForm(problemForm);
        return (drug, takenForm, problemForm);
    }

    private static void AttachOwningDrug(DrugForm form, Drug drug)
    {
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
    }
}
