using System;

namespace DrugsApp.Features.VetDrugFeature.RetrieveVetDrug;

internal static class RetrieveVetDrugMapping
{
    public static RetrieveVetDrugQuery MapToQuery(Guid vetDrugId) => new(vetDrugId);
}

