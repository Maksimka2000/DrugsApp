using System.Net.Http.Headers;
using System.Net.Http.Json;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.Shared;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.Common;

internal static class TempDrugTestFlows
{
    internal static async Task AuthenticateAsAdminAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var login = await TestAuthHelper.LoginAsAdminAsync(client, cancellationToken);
        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static string CreateUniqueNumericProductIdentifier() =>
        Random.Shared.Next(920_000_000, 929_999_999).ToString();

    internal static async Task<Guid> InsertPendingHumanTempDrugAsync(
        IServiceProvider services,
        CancellationToken cancellationToken,
        string? activeSubstanceName = "Paracetamol")
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: $"INT-{Guid.NewGuid():N}"[..24],
            commercialName: $"Integration Temp Drug {Guid.NewGuid():N}",
            activeSubstance: activeSubstanceName,
            typeOfProduct: "Ludzki",
            routeOfAdministration: "Oral use",
            pharmaceuticalFormulation: "Tablet",
            atcCode: "N02BE01",
            leaflet: null,
            educationalMaterials: null,
            dosage: "500 mg",
            linkedImportId: TempDrugTestSeed.ImportBatchId).Value;

        dbContext.TempDrugs.Add(tempDrug);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tempDrug.Id;
    }

    internal static async Task<Guid> InsertPendingVetTempDrugAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: $"INT-VET-{Guid.NewGuid():N}"[..24],
            commercialName: $"Integration Vet Temp Drug {Guid.NewGuid():N}",
            activeSubstance: "Fluralaner",
            typeOfProduct: "Weterynaryjny",
            routeOfAdministration: "Topical",
            pharmaceuticalFormulation: "Spot-on solution",
            atcCode: "QP53BX04",
            leaflet: null,
            educationalMaterials: null,
            dosage: "250 mg",
            linkedImportId: TempDrugTestSeed.ImportBatchId).Value;

        dbContext.TempDrugs.Add(tempDrug);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tempDrug.Id;
    }

    internal static async Task<CreatedImportBatchResponse> UploadImportFileAsync(
        HttpClient client,
        byte[] xlsxBytes,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(xlsxBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "File", "integration-import.xlsx");

        HttpResponseMessage response = await client.PostAsync(
            "/api/temp-drug-imports/file-upload",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedImportBatchResponse? payload = await response.Content.ReadFromJsonAsync<CreatedImportBatchResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Upload import file response body was empty.");
    }
}
