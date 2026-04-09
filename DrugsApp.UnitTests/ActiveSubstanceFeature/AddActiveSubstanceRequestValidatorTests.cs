using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.ActiveSubstanceFeature;

public sealed class AddActiveSubstanceRequestValidatorTests
{
    private readonly AddActiveSubstanceRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_IncludingOptionalGuidelines_ReturnsNoErrors()
    {
        AddActiveSubstanceRequest request = new(
            "Ibuprofen",
            new AddOralMedicationIntakeGuidelinesRequest(
                FoodIntakeType.WithMeal,
                30,
                60,
                "Take after breakfast",
                null,
                null,
                "Drink water",
                true,
                true,
                false));

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Invalid_EmptyOrWhitespaceName_ReturnsValidationError(string name)
    {
        AddActiveSubstanceRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "ActiveSubstanceName");
    }

    [Fact]
    public void Validate_Boundary_ActiveSubstanceNameAtMaxLength_ReturnsNoErrors()
    {
        string name = new string('a', 200);
        AddActiveSubstanceRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_ActiveSubstanceNameExceedsMaxLength_ReturnsValidationError()
    {
        string name = new string('a', 201);
        AddActiveSubstanceRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ActiveSubstanceName");
    }

    [Fact]
    public void Validate_Boundary_WaitBeforeMinutesAtMax_ReturnsNoErrors()
    {
        AddActiveSubstanceRequest request = new(
            "Ibuprofen",
            new AddOralMedicationIntakeGuidelinesRequest(
                FoodIntakeType.WithMeal,
                1440,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_WaitBeforeMinutesAboveMax_ReturnsValidationError()
    {
        AddActiveSubstanceRequest request = new(
            "Ibuprofen",
            new AddOralMedicationIntakeGuidelinesRequest(
                FoodIntakeType.WithMeal,
                2000,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Contains("WaitBeforeMinutes", StringComparison.Ordinal));
    }
}
