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

    [Fact]
    public async Task Validate_WhenMoreThan20SelectedSupplementsOrFoods_ReturnsValidationError()
    {
        var request = new ExecuteRegisteredDrugInteractionToolReportRequest(
            ManualDrugFormIds: null,
            IncludeUserData: false,
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false,
            SelectedSupplementIds: NewIds(21),
            SelectedFoodIds: NewIds(21));

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
        [
            "SelectedSupplementIds must not contain more than 20 items.",
            "SelectedFoodIds must not contain more than 20 items."
        ]);
    }

    [Fact]
    public async Task Validate_WhenTwentyItemsOfEachKind_IsValid()
    {
        var request = new ExecuteRegisteredDrugInteractionToolReportRequest(
            ManualDrugFormIds: NewIds(20),
            IncludeUserData: true,
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false,
            SelectedSupplementIds: NewIds(20),
            SelectedFoodIds: NewIds(20));

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenSelectedListHasDuplicatesOrEmptyGuid_ReturnsValidationErrors()
    {
        Guid duplicate = Guid.NewGuid();
        var request = new ExecuteRegisteredDrugInteractionToolReportRequest(
            ManualDrugFormIds: null,
            IncludeUserData: false,
            ShowAllDrugsInteractions: false,
            ShowOralGuidance: false,
            ShowAllFoodAndSupplementInteractions: false,
            SelectedSupplementIds: [duplicate, duplicate],
            SelectedFoodIds: [Guid.Empty]);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
        [
            "SelectedSupplementIds must be unique.",
            "SelectedFoodIds must not contain empty Guid values."
        ]);
    }

    private static List<Guid> NewIds(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
}
