using DrugsApp.Features.TempDrugFeature.SearchTempDrugs;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.TempDrugFeature;

public sealed class SearchTempDrugsRequestValidatorTests
{
    private readonly SearchTempDrugsRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_ReturnsNoErrors()
    {
        SearchTempDrugsRequest request = new(
            "Drug",
            new List<string> { "pending_review" },
            new List<string> { "ludzki" },
            "created_at",
            "descending",
            0,
            10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_Status_ReturnsValidationError()
    {
        SearchTempDrugsRequest request = new(
            null,
            new List<string> { "bad_status" },
            null,
            "created_at",
            "descending",
            0,
            10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Statuses");
    }

    [Fact]
    public void Validate_Boundary_TakeAtMinimum_ReturnsNoErrors()
    {
        SearchTempDrugsRequest request = new(null, null, null, "created_at", "descending", 0, 1);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Boundary_TakeAtMaximum_ReturnsNoErrors()
    {
        SearchTempDrugsRequest request = new(null, null, null, "created_at", "descending", 0, 40);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_TakeAboveMaximum_ReturnsValidationError()
    {
        SearchTempDrugsRequest request = new(null, null, null, "created_at", "descending", 0, 41);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Take");
    }

    [Fact]
    public void Validate_Boundary_CommercialNameAtMaxLength_ReturnsNoErrors()
    {
        string name = new string('c', 300);
        SearchTempDrugsRequest request = new(name, null, null, "created_at", "descending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_CommercialNameAboveMaxLength_ReturnsValidationError()
    {
        string name = new string('c', 301);
        SearchTempDrugsRequest request = new(name, null, null, "created_at", "descending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CommercialName");
    }
}
