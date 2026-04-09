using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.UnitTests.ActiveSubstanceFeature;

/// <summary>
/// Mapping tests only where <see cref="AddActiveSubstanceMapping"/> applies conditional domain initialization
/// (oral guidelines), not trivial record copies.
/// </summary>
public sealed class AddActiveSubstanceMappingTests
{
    [Fact]
    public void MapToEntity_WhenGuidelinesNull_DoesNotInitializeOralGuidelines()
    {
        AddActiveSubstanceRequest request = new("Paracetamol", null);
        AddActiveSubstanceCommand command = new(request);

        ActiveSubstance entity = command.MapToEntity();

        Assert.Equal("Paracetamol", entity.ActiveSubstanceName);
        Assert.Null(entity.OralMedicationIntakeGuidelines);
    }

    [Fact]
    public void MapToEntity_WhenGuidelinesProvided_InitializesOralGuidelinesOnAggregate()
    {
        AddActiveSubstanceRequest request = new(
            "Paracetamol",
            new AddOralMedicationIntakeGuidelinesRequest(
                FoodIntakeType.AfterMeal,
                10,
                20,
                "Note",
                "Food A",
                "Therapy B",
                "Liquid C",
                true,
                true,
                false));
        AddActiveSubstanceCommand command = new(request);

        ActiveSubstance entity = command.MapToEntity();

        Assert.Equal("Paracetamol", entity.ActiveSubstanceName);
        Assert.NotNull(entity.OralMedicationIntakeGuidelines);
        Assert.Equal(FoodIntakeType.AfterMeal, entity.OralMedicationIntakeGuidelines!.FoodIntakeType);
        Assert.Equal(10, entity.OralMedicationIntakeGuidelines.WaitBeforeMinutes);
    }
}
