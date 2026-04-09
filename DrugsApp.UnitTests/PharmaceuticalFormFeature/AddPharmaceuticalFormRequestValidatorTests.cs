using DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.PharmaceuticalFormFeature;

public sealed class AddPharmaceuticalFormRequestValidatorTests
{
    private readonly AddPharmaceuticalFormRequestValidator _validator = new();

    [Fact]
    public void Validate_Valid_ReturnsNoErrors()
    {
        AddPharmaceuticalFormRequest request = new("Tablet");

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Invalid_EmptyOrWhitespaceName_ReturnsValidationError(string name)
    {
        AddPharmaceuticalFormRequest request = new(name);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FormulationName");
    }

    [Fact]
    public void Validate_Boundary_NameAtMaxLength_ReturnsNoErrors()
    {
        string name = new string('a', 200);
        AddPharmaceuticalFormRequest request = new(name);

        ValidationResult result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Invalid_NameAboveMaxLength_ReturnsValidationError()
    {
        string name = new string('a', 201);
        AddPharmaceuticalFormRequest request = new(name);

        ValidationResult result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FormulationName");
    }
}
