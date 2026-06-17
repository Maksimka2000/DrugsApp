using DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport.Validators;

public sealed class ExecuteRegisteredDrugInteractionToolReportRequestValidatorTests
{
    private readonly ExecuteRegisteredDrugInteractionToolReportRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenMoreThan20ManualForms_ReturnsValidationError()
    {
        var ids = Enumerable.Range(0, 21).Select(_ => Guid.NewGuid()).ToList();
        var request = new ExecuteRegisteredDrugInteractionToolReportRequest(
            ManualDrugFormIds: ids,
            IncludeUserData: false,
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ManualDrugFormIds must not contain more than 20 items.");
    }
}
