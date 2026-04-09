using DrugsApp.Features.DrugFeature.SearchDrugs;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.DrugFeature;

public sealed class SearchDrugsRequestValidatorTests
{
    private readonly SearchDrugsRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_ReturnsNoErrors()
    {
        SearchDrugsRequest request = new("aspirin", new List<string> { "standard" }, "commercial_name", "ascending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_OrderBy_ReturnsValidationError()
    {
        SearchDrugsRequest request = new(null, null, "commercial_name", "up", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderBy");
    }

    [Fact]
    public void Validate_Boundary_TakeAtMinimum_ReturnsNoErrors()
    {
        SearchDrugsRequest request = new(null, null, "commercial_name", "ascending", 0, 1);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Boundary_TakeAtMaximum_ReturnsNoErrors()
    {
        SearchDrugsRequest request = new(null, null, "commercial_name", "ascending", 0, 30);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_TakeAboveMaximum_ReturnsValidationError()
    {
        SearchDrugsRequest request = new(null, null, "commercial_name", "ascending", 0, 31);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Take");
    }

    [Fact]
    public void Validate_Boundary_SearchQueryAtMaxLength_ReturnsNoErrors()
    {
        string query = new string('x', 500);
        SearchDrugsRequest request = new(query, null, "commercial_name", "ascending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_SearchQueryAboveMaxLength_ReturnsValidationError()
    {
        string query = new string('x', 501);
        SearchDrugsRequest request = new(query, null, "commercial_name", "ascending", 0, 10);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SearchQuery");
    }
}
