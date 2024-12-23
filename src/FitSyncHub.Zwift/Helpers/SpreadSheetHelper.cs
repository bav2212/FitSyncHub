using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FitSyncHub.Zwift.Helpers;

internal partial class SpreadSheetHelper
{
    ///<summary>returns an empty cell when a blank cell is encountered
    ///</summary>
    public static IEnumerable<Cell> GetRowCells(Row row)
    {
        var currentCount = 0;

        foreach (var cell in row.Descendants<Cell>())
        {
            var columnName = GetColumnName(cell.CellReference!);

            var currentColumnIndex = ConvertColumnNameToNumber(columnName);

            for (; currentCount < currentColumnIndex; currentCount++)
            {
                yield return new Cell();
            }

            yield return cell;
            currentCount++;
        }
    }

    /// <summary>
    /// Given a cell name, parses the specified cell to get the column name.
    /// </summary>
    /// <param name="cellReference">Address of the cell (ie. B2)</param>
    /// <returns>Column Name (ie. B)</returns>
    public static string GetColumnName(string cellReference)
    {
        // Match the column name portion of the cell name.
        var regex = ColumnNameRegex();
        var match = regex.Match(cellReference);

        return match.Value;
    }

    /// <summary>
    /// Given just the column name (no row index),
    /// it will return the zero based column index.
    /// </summary>
    /// <param name="columnName">Column Name (ie. A or AB)</param>
    /// <returns>Zero based index if the conversion was successful</returns>
    /// <exception cref="ArgumentException">thrown if the given string
    /// contains characters other than uppercase letters</exception>
    public static int ConvertColumnNameToNumber(string columnName)
    {
        var alpha = ColumnNameUpperRegex();
        if (!alpha.IsMatch(columnName))
        {
            throw new ArgumentException($"{columnName} is not a valid column name!");
        }

        var colLetters = columnName.ToCharArray();
        Array.Reverse(colLetters);

        var convertedValue = 0;
        for (var i = 0; i < colLetters.Length; i++)
        {
            var letter = colLetters[i];
            var current = i == 0 ? letter - 65 : letter - 64; // ASCII 'A' = 65
            convertedValue += current * (int)Math.Pow(26, i);
        }

        return convertedValue;
    }

    [GeneratedRegex("[A-Za-z]+")]
    private static partial Regex ColumnNameRegex();

    [GeneratedRegex("^[A-Z]+$")]
    private static partial Regex ColumnNameUpperRegex();
}

