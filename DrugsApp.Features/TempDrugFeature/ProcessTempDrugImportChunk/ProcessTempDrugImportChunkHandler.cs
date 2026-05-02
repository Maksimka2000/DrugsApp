using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.ProcessTempDrugImportChunk;

/// <summary>
/// Internal command for processing a single import data chunk.
/// </summary>
public sealed record ProcessTempDrugImportChunkCommand(
    Guid BatchId,
    List<ImportRowDto> Rows) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Handles per-chunk TempDrug creation and counters updates.
/// </summary>
internal sealed class ProcessTempDrugImportChunkHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugRepository _tempDrugRepository)
    : IRequestHandler<ProcessTempDrugImportChunkCommand, ErrorOr<Success>>
{

    private const int maxCommercialName = 500;
    private const int maxActiveSubstance = 2000;
    private const int maxTypeOfProduct = 100;
    private const int maxRouteOfAdministration = 2000;
    private const int maxPharmaceuticalFormulation = 2000;
    private const int maxAtcCode = 64;
    private const int maxLeaflet = 2000;
    private const int maxEducationalMaterials = 2000;
    private const int maxDosage = 2000;

    public async Task<ErrorOr<Success>> Handle(ProcessTempDrugImportChunkCommand command, CancellationToken cancellationToken)
    {
        // Get the batch and check if it exists and is in progress
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportBatch",
                    ["resourceId"] = command.BatchId.ToString()
                });
        }

        // Check if the batch is cancelled
        if (batch.StatusOfImport == ImportBatchStatus.Cancelled)
        {
            return Result.Success;
        }

        // Initialize the counters
        int importedCount = 0;
        int skippedCount = 0;
        var importedIds = new List<string>();
        var skippedIds = new List<string>();
        var failedIds = new List<string>();
        

        // Iterate over the rows
        foreach (ImportRowDto row in command.Rows)
        {
            // Normalize the product identifier
            string? productIdentifier = Normalize(row.ProductIdentifier);
            if (string.IsNullOrWhiteSpace(productIdentifier))
            {
                failedIds.Add(string.Empty);
                continue;
            }

            // Check if the product identifier already exists
            bool exists = await _tempDrugRepository.ExistsByProductIdentifierAsync(productIdentifier, cancellationToken);
            if (exists)
            {
                skippedCount++;
                skippedIds.Add(productIdentifier);
                continue;
            }

            // Guard lengths against DB constraints to prevent DbUpdateException
            if (!IsWithinLimit(row.CommercialName, maxCommercialName) ||
                !IsWithinLimit(row.ActiveSubstance, maxActiveSubstance) ||
                !IsWithinLimit(row.TypeOfProduct, maxTypeOfProduct) ||
                !IsWithinLimit(row.RouteOfAdministration, maxRouteOfAdministration) ||
                !IsWithinLimit(row.PharmaceuticalFormulation, maxPharmaceuticalFormulation) ||
                !IsWithinLimit(row.AtcCode, maxAtcCode) ||
                !IsWithinLimit(row.Leaflet, maxLeaflet) ||
                !IsWithinLimit(row.EducationalMaterials, maxEducationalMaterials) ||
                !IsWithinLimit(row.Dosage, maxDosage))
            {
                failedIds.Add(productIdentifier);
                continue;
            }

            // Create the temp drug
            ErrorOr<TempDrug> tempDrugResult = TempDrug.CreateImported(
                productIdentifier: productIdentifier,
                commercialName: Normalize(row.CommercialName),
                activeSubstance: Normalize(row.ActiveSubstance),
                typeOfProduct: Normalize(row.TypeOfProduct),
                routeOfAdministration: Normalize(row.RouteOfAdministration),
                pharmaceuticalFormulation: Normalize(row.PharmaceuticalFormulation),
                atcCode: Normalize(row.AtcCode),
                leaflet: Normalize(row.Leaflet),
                educationalMaterials: Normalize(row.EducationalMaterials),
                dosage: Normalize(row.Dosage),
                linkedImportId: command.BatchId);

            // If the temp drug creation failed, add the product identifier to the failed ids
            if (tempDrugResult.IsError)
            {
                failedIds.Add(productIdentifier);
                continue;
            }

            // Add the temp drug to the repository
            await _tempDrugRepository.AddAsync(tempDrugResult.Value, cancellationToken);
            importedCount++;
            importedIds.Add(productIdentifier);
        }

        // Record the chunk imported
        batch.RecordChunkImported(importedCount, importedIds, skippedCount, skippedIds, failedIds);
        return Result.Success;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static bool IsWithinLimit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.Trim().Length <= maxLength;
    }
}

