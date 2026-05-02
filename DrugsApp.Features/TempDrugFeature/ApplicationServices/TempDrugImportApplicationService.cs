using System.Text.Json;
using ClosedXML.Excel;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.CompleteTempDrugImport;
using DrugsApp.Features.TempDrugFeature.ProcessTempDrugImportChunk;
using DrugsApp.Features.TempDrugFeature.Shared;
using MediatR;
using System.Linq;
using ErrorOr;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

internal sealed class TempDrugImportApplicationService(
    ITempDrugImportBatchRepository _batchRepository,
    IBlobService _blobService,
    IMediator _mediator,
    IUnitOfWork _unitOfWork) : ITempDrugImportApplicationService
{
    private const int ChunkSize = 100;

    public async Task RunImportAsync(Guid batchId, CancellationToken cancellationToken)
    {
        try
        {
            // validate batch
            TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
            if (batch is null || batch.StatusOfImport != ImportBatchStatus.InProgress)
            {
                await MarkBatchErrorAsync(batchId, "Batch is not in progress or does not exist", cancellationToken);
                return;
            }

            // download file
            FileResponse fileResponse = await _blobService.DownloadAsync(batch.BlobFileId, cancellationToken);
            await using Stream stream = fileResponse.Stream;
            var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet = workbook.Worksheets.First();
            IXLRange? usedRange = worksheet.RangeUsed();

            // validate used range
            if (usedRange is null)
            {
                // Map to command 
                CompleteTempDrugImportCommand completeTempDrugImportCommand = CompleteTempDrugImportMapping.MapToCommand(batchId);
                // Call the handler: CompleteTempDrugImport 
                ErrorOr<Success> completeResult = await _mediator.Send(completeTempDrugImportCommand, cancellationToken);
                if (completeResult.IsError)
                {
                    await MarkBatchErrorAsync(batchId, completeResult.FirstError.Description, cancellationToken);
                }
                return;
            }

            // parse mapping plan
            Dictionary<string, string> mappingPlan = ParseMappingPlan(batch.MappingPlanJson);
            Dictionary<string, int> headerIndexes = BuildHeaderIndexes(usedRange.FirstRow());

            // process rows
            var rowsBuffer = new List<ImportRowDto>(ChunkSize);
            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
            {
                rowsBuffer.Add(MapRow(row, mappingPlan, headerIndexes));

                if (rowsBuffer.Count < ChunkSize)
                {
                    continue;
                }
                // ProcessTempDrugImportChunkCommand map to command 
                ProcessTempDrugImportChunkCommand chunkCommand = rowsBuffer.MapToCommand(batchId);
                // ProcessTempDrugImportChunkCommand call 
                ErrorOr<Success> chunkResult = await _mediator.Send(chunkCommand, cancellationToken);
                if (chunkResult.IsError)
                {
                    await MarkBatchErrorAsync(batchId, chunkResult.FirstError.Description, cancellationToken);
                    return;
                }

                rowsBuffer.Clear();
            }

            // process remaining rows
            if (rowsBuffer.Count > 0)
            {
                // ProcessTempDrugImportChunkCommand map to command 
                ProcessTempDrugImportChunkCommand chunkCommand = rowsBuffer.MapToCommand(batchId);
                // ProcessTempDrugImportChunkCommand call 
                ErrorOr<Success> chunkResult = await _mediator.Send(chunkCommand, cancellationToken);
                if (chunkResult.IsError)
                {
                    await MarkBatchErrorAsync(batchId, chunkResult.FirstError.Description, cancellationToken);
                    return;
                }
            }

            // validate final batch
            TempDrugImportBatch? finalBatch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
            if (finalBatch is null || finalBatch.StatusOfImport == ImportBatchStatus.Cancelled)
            {
                return;
            }

            // Map to command 
            CompleteTempDrugImportCommand completeTempDrugImportCommand2 = CompleteTempDrugImportMapping.MapToCommand(batchId);
            // Call the handler: CompleteTempDrugImport 
            ErrorOr<Success> completeImportResult = await _mediator.Send(completeTempDrugImportCommand2, cancellationToken);
            
            if (completeImportResult.IsError)
            {
                await MarkBatchErrorAsync(batchId, completeImportResult.FirstError.Description, cancellationToken);
                return;
            }
        }
        catch (Exception exception)
        {
            await MarkBatchErrorAsync(batchId, exception.Message, cancellationToken);
        }
    }

    private async Task MarkBatchErrorAsync(Guid batchId, string? errorNote, CancellationToken cancellationToken)
    {
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return;
        }

        batch.MarkError(errorNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }



    private static Dictionary<string, string> ParseMappingPlan(string? mappingPlanJson)
    {
        if (string.IsNullOrWhiteSpace(mappingPlanJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(mappingPlanJson)
               ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, int> BuildHeaderIndexes(IXLRangeRow headerRow)
    {
        var headerIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (IXLCell cell in headerRow.CellsUsed())
        {
            string value = cell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!headerIndexes.ContainsKey(value))
            {
                headerIndexes[value] = cell.Address.ColumnNumber;
            }
        }

        return headerIndexes;
    }

    private static ImportRowDto MapRow(
        IXLRow row,
        Dictionary<string, string> mappingPlan,
        Dictionary<string, int> headerIndexes)
    {
        string? GetValue(string fieldName)
        {
            // Try to get the source column name from the mapping plan
            if (!mappingPlan.TryGetValue(fieldName, out string? sourceColumnName))
            {
                return null;
            }

            // Check if the source column name is not empty
            if (string.IsNullOrWhiteSpace(sourceColumnName))
            {
                return null;
            }

            // Try to get the column index from the header indexes
            if (!headerIndexes.TryGetValue(sourceColumnName, out int columnIndex))
            {
                return null;
            }

            // Get the value from the cell
            string value = row.Cell(columnIndex).GetString().Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return new ImportRowDto(
            ProductIdentifier: GetValue("ProductIdentifier"),
            CommercialName: GetValue("CommercialName"),
            ActiveSubstance: GetValue("ActiveSubstance"),
            TypeOfProduct: GetValue("TypeOfProduct"),
            RouteOfAdministration: GetValue("RouteOfAdministration"),
            PharmaceuticalFormulation: GetValue("PharmaceuticalFormulation"),
            AtcCode: GetValue("ATCCode"),
            Leaflet: GetValue("Leaflet"),
            EducationalMaterials: GetValue("EducationalMaterials"),
            Dosage: GetValue("Dosage"));
    }
}

