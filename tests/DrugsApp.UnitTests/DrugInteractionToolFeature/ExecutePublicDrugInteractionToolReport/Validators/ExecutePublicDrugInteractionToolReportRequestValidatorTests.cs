using DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport.Validators;

public sealed class ExecutePublicDrugInteractionToolReportRequestValidatorTests
{
    private readonly ExecutePublicDrugInteractionToolReportRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenMoreThan3ManualForms_ReturnsValidationError()
    {
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ManualDrugFormIds must not contain more than 3 items.");
    }

    [Fact]
    public async Task Validate_WhenDuplicateDrugFormIds_ReturnsValidationError()
    {
        var formId = Guid.NewGuid();
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: [formId, formId],
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ManualDrugFormIds must be unique.");
    }

    [Fact]
    public async Task Validate_WhenEmptyGuid_ReturnsValidationError()
    {
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: [Guid.Empty],
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ManualDrugFormIds must not contain empty Guid values.");
    }
}
