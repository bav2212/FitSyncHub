using System.Data;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FitSyncHub.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Common.Services;

public sealed class ExcelReader
{
    public static readonly string HyperlinkColumnName = "__Hyperlink";
    private readonly ILogger<ExcelReader> _logger;

    public ExcelReader(ILogger<ExcelReader> logger)
    {
        _logger = logger;
    }

    public DataTable Read(string filePath, string? sheetName = null)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _logger.LogInformation("Started reading file: {filePath}", filePath);

        var result = ReadImplementation(filePath, sheetName);

        _logger.LogInformation("Finished reading file: {filePath}, elapsed {ElapsedMilliseconds} milliseconds", filePath, stopwatch.ElapsedMilliseconds);

        return result;
    }

    private DataTable ReadImplementation(string fileName, string? sheetName)
    {
        var table = new DataTable();
        using var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false);
        var workbookPart = spreadSheetDocument.WorkbookPart!;
        var worksheetPart = GetWorksheetPart(workbookPart, sheetName);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
        var hyperlinks = worksheetPart.RootElement!.Descendants<Hyperlinks>().First().Cast<Hyperlink>().ToArray();
        var hyperlinkRelationships = worksheetPart.HyperlinkRelationships.ToArray();

        if (sheetData.Elements<Row>().ToArray() is not [var headerRow, .. var rows])
        {
            throw new InvalidOperationException();
        }

        var columns = CreateDataColumns(spreadSheetDocument, headerRow).ToArray();
        table.Columns.AddRange(columns);

        var columnsCount = table.Columns.Count;

        foreach (var row in rows)
        {
            var tempRow = table.NewRow();

            // should use custom method to get row cells, cause row.Elements<Cell>() doesn't return empty cells
            var rowCells = SpreadSheetHelper.GetRowCells(row);
            var i = 0;
            foreach (var cell in rowCells)
            {
                if (i >= columnsCount)
                {
                    _logger.LogWarning("Cell item {Cell} with index {index} exists out of columns", cell.CellReference, i);
                    break;
                }

                tempRow[i++] = GetCellValue(spreadSheetDocument, cell);
                if (GetCellHyperlink(cell, hyperlinks, hyperlinkRelationships) is { } hyperlink)
                {
                    tempRow[HyperlinkColumnName] = hyperlink;
                }
            }

            // skip empty rows
            if (i > 0)
            {
                table.Rows.Add(tempRow);
            }
        }
        return table;
    }

    private static string? GetCellHyperlink(Cell cell, Hyperlink[] hyperlinks, HyperlinkRelationship[] hyperlinkRelationships)
    {
        if (cell.CellReference is null)
        {
            return null;
        }

        // get the Hyperlink object "behind" the cell
        var hyperlink = hyperlinks.SingleOrDefault(i => i.Reference!.Value == cell.CellReference.Value);

        if (hyperlink is null)
        {
            return null;
        }

        // if the hyperlink has an anchor, the anchor will be stored without the # in hyperlink.Location
        string location = hyperlink.Location!;

        // the URI is stored in the HyperlinkRelationship
        var hyperlinkRelationship = hyperlinkRelationships.SingleOrDefault(i => i.Id == hyperlink.Id);
        var url = hyperlinkRelationship!.Uri.ToString();

        return string.IsNullOrWhiteSpace(location) ? url : $"{url}#{location}";
    }

    private static IEnumerable<DataColumn> CreateDataColumns(SpreadsheetDocument spreadSheetDocument, Row headerRow)
    {
        foreach (var cell in headerRow.Cast<Cell>())
        {
            var columnName = GetCellValue(spreadSheetDocument, cell)
                ?? throw new Exception("Column name can't be null");
            yield return new DataColumn(columnName);
        }

        yield return new DataColumn(HyperlinkColumnName);
    }

    private static WorksheetPart GetWorksheetPart(WorkbookPart workbookPart, string? sheetName)
    {
        if (sheetName is null)
        {
            return workbookPart.WorksheetParts.First();
        }

        var sheet = workbookPart.Workbook.Sheets!.Elements<Sheet>()
            .FirstOrDefault(s => s.Name == sheetName)
            ?? throw new InvalidOperationException($"Sheet with name ${sheetName} not found");

        return (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
    }

    private static string? GetCellValue(SpreadsheetDocument document, Cell cell)
    {
        if (cell.CellValue is null)
        {
            return cell.DataType != null && cell.DataType.Value == CellValues.InlineString
                ? (cell.InlineString?.Text?.Text)
                : default;
        }

        var value = cell.CellValue.InnerXml;
        return cell.DataType != null && cell.DataType.Value == CellValues.SharedString
            ? document.WorkbookPart!.SharedStringTablePart!.SharedStringTable.ChildElements[int.Parse(value)].InnerText
            : value;
    }
}
