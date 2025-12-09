namespace DrugsApp.Features.TestFeature.RetrieveTest;

internal static class RetrieveTestMapping
{
    public static GetTestByIdQuery MapToQuery(Guid testId)
    {
        return new GetTestByIdQuery(testId);
    }
}

