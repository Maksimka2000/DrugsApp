using ClosedXML.Excel;

namespace DrugsApp.IntegrationTests.Common;

internal static class TempDrugTestExcel
{
    internal static byte[] CreateSampleWorkbookBytes()
    {
        using var workbook = new XLWorkbook();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Import");

        worksheet.Cell(1, 1).Value = "CommercialName";
        worksheet.Cell(1, 2).Value = "ProductIdentifier";
        worksheet.Cell(1, 3).Value = "ActiveSubstance";
        worksheet.Cell(2, 1).Value = "Integration import candidate";
        worksheet.Cell(2, 2).Value = "123456";
        worksheet.Cell(2, 3).Value = "Paracetamol";

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }
}
