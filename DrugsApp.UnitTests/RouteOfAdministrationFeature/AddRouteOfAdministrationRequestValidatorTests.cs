using DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.RouteOfAdministrationFeature;

public sealed class AddRouteOfAdministrationRequestValidatorTests
{
    private readonly AddRouteOfAdministrationRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_ReturnsNoErrors()
    {
        AddRouteOfAdministrationRequest request = new("Oral", true);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Invalid_EmptyOrWhitespaceName_ReturnsValidationError(string name)
    {
        AddRouteOfAdministrationRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteOfAdministrationName");
    }

    [Fact]
    public void Validate_Boundary_NameAtMaxLength_ReturnsNoErrors()
    {
        string name = new string('a', 200);
        AddRouteOfAdministrationRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_NameAboveMaxLength_ReturnsValidationError()
    {
        string name = new string('a', 201);
        AddRouteOfAdministrationRequest request = new(name, null);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteOfAdministrationName");
    }
}
