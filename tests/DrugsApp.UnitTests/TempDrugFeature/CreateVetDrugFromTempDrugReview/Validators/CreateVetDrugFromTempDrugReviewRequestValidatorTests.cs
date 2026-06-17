using DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateVetDrugFromTempDrugReview.Validators;

public sealed class CreateVetDrugFromTempDrugReviewRequestValidatorTests
{
    private readonly CreateVetDrugFromTempDrugReviewRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenCommercialNameEmpty_ReturnsValidationError()
    {
        var request = new CreateVetDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "",
            TypeOfProduct: "Weterynaryjny",
            ProductIdentifier: "100");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateVetDrugFromTempDrugReviewRequest.CommercialName));
    }

    [Fact]
    public async Task Validate_WhenProductIdentifierNotPositiveInteger_ReturnsValidationError()
    {
        var request = new CreateVetDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "VetApap",
            TypeOfProduct: "Weterynaryjny",
            ProductIdentifier: "-1");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ProductIdentifier must be a positive integer.");
    }

    [Fact]
    public async Task Validate_WhenNoteExceedsMaxLength_ReturnsValidationError()
    {
        var request = new CreateVetDrugFromTempDrugReviewRequest(
            Note: new string('n', 2001),
            CommercialName: "VetApap",
            TypeOfProduct: "Weterynaryjny",
            ProductIdentifier: "100");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateVetDrugFromTempDrugReviewRequest.Note));
    }

    [Fact]
    public async Task Validate_WhenValidRequest_IsValid()
    {
        var request = new CreateVetDrugFromTempDrugReviewRequest(
            Note: "review note",
            CommercialName: "VetApap",
            TypeOfProduct: "Weterynaryjny",
            ProductIdentifier: "100");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
