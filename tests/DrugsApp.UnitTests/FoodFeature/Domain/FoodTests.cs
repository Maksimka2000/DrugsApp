using DrugsApp.Domain.FoodFeature.Entities;
using FluentAssertions;

namespace DrugsApp.UnitTests.FoodFeature.Domain;

public sealed class FoodTests
{
    [Fact]
    public void Create_BuildsSearchTextFromNameAndAliases()
    {
        Food food = Food.Create("Grapefruit", ["pink grapefruit", "citrus"]).Value;

        food.SearchText.Should().Be("grapefruit pink grapefruit citrus");
    }

    [Fact]
    public void Create_BuildsSearchOutputTextWithAliasesInParentheses()
    {
        Food food = Food.Create("Aged cheese", ["cheddar", "parmesan"]).Value;

        food.BuildSearchOutputText().Should().Be("aged cheese (cheddar, parmesan)");
    }

    [Fact]
    public void Create_BuildsSearchOutputTextWithoutParenthesesWhenNoAliases()
    {
        Food food = Food.Create("Almonds", []).Value;

        food.BuildSearchOutputText().Should().Be("almonds");
    }

    [Fact]
    public void ReplaceAliases_RebuildsSearchText()
    {
        Food food = Food.Create("Grapefruit", ["pink grapefruit"]).Value;

        food.ReplaceAliases(["pink grapefruit", "citrus"]);

        food.SearchText.Should().Be("grapefruit pink grapefruit citrus");
        food.BuildSearchOutputText().Should().Be("grapefruit (pink grapefruit, citrus)");
    }

    [Fact]
    public void UpdateName_RebuildsSearchText()
    {
        Food food = Food.Create("Milk", ["dairy"]).Value;

        food.UpdateName("Cow milk");

        food.Name.Should().Be("Cow milk");
        food.SearchText.Should().Be("cow milk dairy");
    }
}
