using System;
using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;

internal static class UpdateTempDrugStatusMapping
{
    public static UpdateTempDrugStatusCommand MapToCommand(
        this UpdateTempDrugStatusRequest request,
        Guid tempDrugId) => new(
        TempDrugId: tempDrugId,
        Request: request);

    public static UpdatedTempDrugStatusResponse MapToResponse(this TempDrug tempDrug) => new(
        Id: tempDrug.Id,
        Status: tempDrug.Status,
        Note: tempDrug.Note ?? string.Empty);
}


