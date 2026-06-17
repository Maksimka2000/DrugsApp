using DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.PatientSupervisionFeature.UpdateSupervisionAccessLevel.Validators;

public sealed class UpdateSupervisionAccessLevelRequestValidatorTests
{
    private readonly UpdateSupervisionAccessLevelRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenAccessLevelCodeInvalid_ReturnsValidationError()
    {
        var request = new UpdateSupervisionAccessLevelRequest("invalid_level");

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "access_level_code must be one of view_and_modify|view_only|restricted_view|block_access.");
    }
}
