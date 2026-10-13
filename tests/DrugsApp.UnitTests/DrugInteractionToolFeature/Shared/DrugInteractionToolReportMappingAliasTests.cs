using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugInteractionToolReportMappingAliasTests
{
    [Fact]
    public void BuildNameWithAliases_WhenNoAliases_ReturnsNameOnly()
    {
        string result = DrugInteractionToolReportMapping.BuildNameWithAliases("Wheat", []);

        result.Should().Be("Wheat");
    }

    [Fact]
    public void BuildNameWithAliases_WhenAliasesPresent_AppendsAllInParentheses()
    {
        string result = DrugInteractionToolReportMapping.BuildNameWithAliases("Wheat", ["gluten grain", "durum"]);

        result.Should().Be("Wheat (gluten grain, durum)");
    }

    [Fact]
    public void BuildFoodOrSupplementDisplayLine_WithAliasedName_KeepsRemainingSegments()
    {
        string name = DrugInteractionToolReportMapping.BuildNameWithAliases("Wheat", ["durum"]);

        string line = DrugInteractionToolReportMapping.BuildFoodOrSupplementDisplayLine(
            "Amoxicillin",
            name,
            isTakenNow: false,
            "High-fat meal increases peak concentration.",
            stomachText: null,
            "Moderate");

        line.Should().Be("Amoxicillin => Wheat (durum): High-fat meal increases peak concentration. Moderate");
    }
}
