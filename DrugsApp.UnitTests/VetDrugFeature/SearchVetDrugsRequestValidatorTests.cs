using DrugsApp.Features.VetDrugFeature.SearchVetDrugs;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.VetDrugFeature;

public sealed class SearchVetDrugsRequestValidatorTests
{
    private readonly SearchVetDrugsRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_ReturnsNoErrors()
    {
        SearchVetDrugsRequest request = new("Rimadyl", "descending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_Order_ReturnsValidationError()
    {
        SearchVetDrugsRequest request = new(null, "sideways", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Order");
    }

    [Fact]
    public void Validate_Boundary_TakeAtMinimum_ReturnsNoErrors()
    {
        SearchVetDrugsRequest request = new(null, "ascending", 0, 1);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Boundary_TakeAtMaximum_ReturnsNoErrors()
    {
        SearchVetDrugsRequest request = new(null, "ascending", 0, 100);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_TakeAboveMaximum_ReturnsValidationError()
    {
        SearchVetDrugsRequest request = new(null, "ascending", 0, 101);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Take");
    }

    [Fact]
    public void Validate_Boundary_NameAtMaxLength_ReturnsNoErrors()
    {
        string name = new string('n', 300);
        SearchVetDrugsRequest request = new(name, "ascending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_NameAboveMaxLength_ReturnsValidationError()
    {
        string name = new string('n', 301);
        SearchVetDrugsRequest request = new(name, "ascending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}
