using System.Text.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugInteractionToolReportMappingRegularIntakeTests
{
    private static readonly DateOnly Today = new(2026, 9, 28);
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void MapRegularIntake_WhenSourceIsManual_ReturnsNull()
    {
        (Drug drug, DrugForm form) = CreateDrug();
        ClientRegularDrug regular = CreateRegular(
            drug,
            form,
            startDate: Today,
            endDate: Today.AddDays(1),
            intakeTimes: [new TimeOnly(8, 0)]);

        RegularDrugIntakeReadModel? intake = DrugInteractionToolReportMapping.MapRegularIntake(
            SelectedDrugSource.Manual,
            regular);

        intake.Should().BeNull();
    }

    [Fact]
    public void MapRegularIntake_WhenUserRegularHasTimesAndDates_ReturnsPayload()
    {
        (Drug drug, DrugForm form) = CreateDrug();
        ClientRegularDrug regular = CreateRegular(
            drug,
            form,
            startDate: new DateOnly(2026, 9, 9),
            endDate: new DateOnly(2026, 9, 10),
            intakeTimes: [new TimeOnly(16, 0), new TimeOnly(8, 0)]);

        RegularDrugIntakeReadModel? intake = DrugInteractionToolReportMapping.MapRegularIntake(
            SelectedDrugSource.UserRegular,
            regular);

        intake.Should().NotBeNull();
        intake!.IntakeTimes.Should().Equal("08:00", "16:00");
        intake.StartDate.Should().Be("2026-09-09");
        intake.EndDate.Should().Be("2026-09-10");
        intake.IsOngoing.Should().BeFalse();
    }

    [Fact]
    public void MapRegularIntake_WhenOngoingWithoutTimes_ReturnsEmptyTimes()
    {
        (Drug drug, DrugForm form) = CreateDrug();
        ClientRegularDrug regular = CreateRegular(
            drug,
            form,
            startDate: null,
            endDate: null,
            intakeTimes: null);

        RegularDrugIntakeReadModel? intake = DrugInteractionToolReportMapping.MapRegularIntake(
            SelectedDrugSource.ManualAndUserRegular,
            regular);

        intake.Should().NotBeNull();
        intake!.IntakeTimes.Should().BeEmpty();
        intake.StartDate.Should().BeNull();
        intake.EndDate.Should().BeNull();
        intake.IsOngoing.Should().BeTrue();
    }

    [Fact]
    public void MapSpecificDrugDetail_WhenManual_OmitsRegularIntake()
    {
        (Drug drug, DrugForm form) = CreateDrug();
        ClientRegularDrug regular = CreateRegular(
            drug,
            form,
            startDate: Today,
            endDate: Today,
            intakeTimes: [new TimeOnly(8, 0)]);

        SpecificDrugDetailReadModel detail = DrugInteractionToolReportMapping.MapSpecificDrugDetail(
            drug,
            form,
            SelectedDrugSource.Manual,
            DrugInteractionToolReportMapping.MapRegularIntakeSnapshot(regular));

        detail.SelectedDrugSource.Should().Be(SelectedDrugSource.Manual);
        detail.RegularIntake.Should().BeNull();
    }

    [Fact]
    public void SelectedDrugSource_SerializesAsApiStrings()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        JsonSerializer.Serialize(SelectedDrugSource.Manual, options).Should().Be("\"manual\"");
        JsonSerializer.Serialize(SelectedDrugSource.UserRegular, options).Should().Be("\"user_regular\"");
        JsonSerializer.Serialize(SelectedDrugSource.ManualAndUserRegular, options)
            .Should().Be("\"manual_and_user_regular\"");
    }

    private static (Drug Drug, DrugForm Form) CreateDrug()
    {
        Drug drug = Drug.Create("Amotaks", "Ludzki", DrugType.Standard, [], drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "500 mg");
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
        drug.AddDrugForm(form);
        return (drug, form);
    }

    private static ClientRegularDrug CreateRegular(
        Drug drug,
        DrugForm form,
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<TimeOnly>? intakeTimes)
    {
        ClientRegularDrug regular = ClientRegularDrug.Create(
            UserId,
            drug.Id,
            form.Id,
            startDate,
            endDate,
            intakeTimes,
            Today).Value;
        regular.AttachCatalogReferences(drug, form);
        return regular;
    }
}
