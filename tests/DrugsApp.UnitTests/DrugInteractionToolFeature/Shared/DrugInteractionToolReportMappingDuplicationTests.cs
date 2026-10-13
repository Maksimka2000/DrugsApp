using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugInteractionToolReportMappingDuplicationTests
{
    [Fact]
    public void MapDuplicationDrugSummaries_WhenManual_ReturnsSelectedOnly()
    {
        (Drug drug, DrugForm form) = CreateDrug();

        IReadOnlyList<DuplicationDrugSummaryReadModel> summaries =
            DrugInteractionToolReportMapping.MapDuplicationDrugSummaries(drug, form, SelectedDrugSource.Manual);

        summaries.Select(summary => summary.Source).Should().Equal(DuplicationDrugSource.Selected);
    }

    [Fact]
    public void MapDuplicationDrugSummaries_WhenPublicReport_ReturnsSelectedOnly()
    {
        (Drug drug, DrugForm form) = CreateDrug();

        IReadOnlyList<DuplicationDrugSummaryReadModel> summaries =
            DrugInteractionToolReportMapping.MapDuplicationDrugSummaries(drug, form, source: null);

        summaries.Select(summary => summary.Source).Should().Equal(DuplicationDrugSource.Selected);
    }

    [Fact]
    public void MapDuplicationDrugSummaries_WhenUserRegular_ReturnsTakingNowOnly()
    {
        (Drug drug, DrugForm form) = CreateDrug();

        IReadOnlyList<DuplicationDrugSummaryReadModel> summaries =
            DrugInteractionToolReportMapping.MapDuplicationDrugSummaries(drug, form, SelectedDrugSource.UserRegular);

        summaries.Select(summary => summary.Source).Should().Equal(DuplicationDrugSource.TakingNow);
    }

    [Fact]
    public void MapDuplicationDrugSummaries_WhenManualAndUserRegular_ReturnsBothRoles()
    {
        (Drug drug, DrugForm form) = CreateDrug();

        IReadOnlyList<DuplicationDrugSummaryReadModel> summaries =
            DrugInteractionToolReportMapping.MapDuplicationDrugSummaries(
                drug,
                form,
                SelectedDrugSource.ManualAndUserRegular);

        summaries.Select(summary => summary.Source)
            .Should().Equal(DuplicationDrugSource.TakingNow, DuplicationDrugSource.Selected);
        summaries.Should().OnlyContain(summary => summary.DrugId == drug.Id);
    }

    [Fact]
    public void MapDuplicationDrugSummaries_IncludesFormAndDosageInSummaryText()
    {
        (Drug drug, DrugForm form) = CreateDrug();

        DuplicationDrugSummaryReadModel summary = DrugInteractionToolReportMapping
            .MapDuplicationDrugSummaries(drug, form, SelectedDrugSource.Manual)
            .Single();

        summary.OnlyDrugSummaryText.Should().Be("Amotaks (Amoxicillin) | 500 mg");
    }

    private static (Drug Drug, DrugForm Form) CreateDrug()
    {
        Drug drug = Drug.Create(
            "Amotaks",
            "Ludzki",
            DrugType.Standard,
            [ActiveSubstance.Create("Amoxicillin")],
            drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "500 mg");
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
        drug.AddDrugForm(form);
        return (drug, form);
    }
}
