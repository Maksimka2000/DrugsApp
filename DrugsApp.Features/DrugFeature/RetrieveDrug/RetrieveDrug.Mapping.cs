using System;

namespace DrugsApp.Features.DrugFeature.RetrieveDrug;

internal static class RetrieveDrugMapping
{
    public static RetrieveDrugQuery MapToQuery(Guid drugId) => new(drugId);
}

