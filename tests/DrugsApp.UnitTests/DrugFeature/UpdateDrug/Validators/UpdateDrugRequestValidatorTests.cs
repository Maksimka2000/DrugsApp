using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.UpdateDrug;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.DrugFeature.UpdateDrug.Validators;

public sealed class UpdateDrugRequestValidatorTests
{
    private readonly UpdateDrugRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenNewActiveSubstanceIdsEmpty_ReturnsValidationError()
    {
        var request = new UpdateDrugRequest(
            CommercialName: null,
            NewActiveSubstanceIds: [],
            TypeOfDrug: null);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Drug must have at least one active substance.");
    }

    [Fact]
    public async Task Validate_WhenDuplicateActiveSubstanceIds_ReturnsValidationError()
    {
        var substanceId = Guid.NewGuid();
        var request = new UpdateDrugRequest(
            CommercialName: null,
            NewActiveSubstanceIds: [substanceId, substanceId],
            TypeOfDrug: null);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Duplicate active substance ids are not allowed.");
    }
}
