using DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport.Validators;

public sealed class ExecutePublicDrugInteractionToolReportRequestValidatorTests
{
    private readonly ExecutePublicDrugInteractionToolReportRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenMoreThan5ManualForms_ReturnsValidationError()
    {
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: NewIds(6),
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ManualDrugFormIds must not contain more than 5 items.");
    }

    [Fact]
    public async Task Validate_WhenFiveItemsOfEachKind_IsValid()
    {
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: NewIds(5),
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false,
            SelectedSupplementIds: NewIds(5),
            SelectedFoodIds: NewIds(5));

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenMoreThan5SelectedSupplementsOrFoods_ReturnsValidationError()
    {
        var request = new ExecutePublicDrugInteractionToolReportRequest(
            ManualDrugFormIds: null,
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false,
            SelectedSupplementIds: NewIds(6),
            SelectedFoodIds: NewIds(6));

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
        [
            "SelectedSupplementIds must not contain more than 5 items.",
            "SelectedFoodIds must not contain more than 5 items."
        ]);
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

    private static List<Guid> NewIds(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
}
