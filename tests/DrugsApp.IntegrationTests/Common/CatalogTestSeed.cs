namespace DrugsApp.IntegrationTests.Common;

internal static class CatalogTestSeed
{
    internal static readonly Guid NurofenDrugId = Guid.Parse("a0000000-0000-4000-8000-000000000001");

    internal static readonly Guid NurofenDrugFormId = Guid.Parse("a1000000-0000-4000-8000-000000000001");

    internal static readonly Guid IbuprofenId = Guid.Parse("f2000000-0000-4000-8000-000000000001");

    internal static readonly Guid ParacetamolId = Guid.Parse("f2000000-0000-4000-8000-000000000002");

    internal static readonly Guid NsaidGroupId = Guid.Parse("f2500000-0000-4000-8000-000000000001");

    internal static readonly Guid GrapefruitFoodId = Guid.Parse("b0000000-0000-4000-8000-000000000001");

    internal static readonly Guid MilkFoodId = Guid.Parse("b0000000-0000-4000-8000-000000000002");

    internal static readonly Guid PeanutsFoodId = Guid.Parse("b0000000-0000-4000-8000-000000000003");

    internal static readonly Guid EggsFoodId = Guid.Parse("b0000000-0000-4000-8000-000000000004");

    internal static readonly Guid ShellfishFoodId = Guid.Parse("b0000000-0000-4000-8000-000000000005");

    internal static readonly Guid CitrusFoodGroupId = Guid.Parse("f2600000-0000-4000-8000-000000000001");

    internal static readonly Guid[] CitrusFoodGroupMemberIds =
    [
        GrapefruitFoodId,
        MilkFoodId,
        PeanutsFoodId,
        EggsFoodId,
        ShellfishFoodId
    ];
}
