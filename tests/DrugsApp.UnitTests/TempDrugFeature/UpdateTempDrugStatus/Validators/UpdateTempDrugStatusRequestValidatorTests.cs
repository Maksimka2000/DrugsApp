using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.UpdateTempDrugStatus.Validators;

public sealed class UpdateTempDrugStatusRequestValidatorTests
{
    private readonly UpdateTempDrugStatusRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenStatusInvalid_ReturnsValidationError()
    {
        var request = new UpdateTempDrugStatusRequest(
            Status: (TempDrugStatus)999,
            Note: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTempDrugStatusRequest.Status));
    }

    [Fact]
    public async Task Validate_WhenNoteExceedsMaxLength_ReturnsValidationError()
    {
        var request = new UpdateTempDrugStatusRequest(
            Status: TempDrugStatus.ReviewLater,
            Note: new string('n', 501));

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTempDrugStatusRequest.Note));
    }

    [Fact]
    public async Task Validate_WhenReviewLaterWithoutNote_IsValid()
    {
        var request = new UpdateTempDrugStatusRequest(
            Status: TempDrugStatus.ReviewLater,
            Note: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
