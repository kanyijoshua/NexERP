using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// The .xlsx writer. An Excel file that will not open is a support call rather than a bug report,
/// so the parts Excel insists on are checked here.
/// </summary>
public class SpreadsheetWriter_Tests
{
    [Theory]
    [InlineData(0, 1, "A1")]
    [InlineData(1, 1, "B1")]
    [InlineData(25, 3, "Z3")]
    [InlineData(26, 1, "AA1")]
    [InlineData(27, 1, "AB1")]
    [InlineData(51, 1, "AZ1")]
    [InlineData(52, 1, "BA1")]
    public void Names_Cells_The_Way_Excel_Does(int column, int row, string expected)
    {
        SpreadsheetWriter.Reference(column, row).ShouldBe(expected);
    }

    [Fact]
    public void Writes_A_Workbook_With_The_Parts_Excel_Requires()
    {
        var bytes = Write();

        using var archive = new ZipArchive(new MemoryStream(bytes));
        var names = archive.Entries.Select(e => e.FullName).ToList();

        names.ShouldContain("[Content_Types].xml");
        names.ShouldContain("_rels/.rels");
        names.ShouldContain("xl/workbook.xml");
        names.ShouldContain("xl/_rels/workbook.xml.rels");
        names.ShouldContain("xl/styles.xml");
        names.ShouldContain("xl/worksheets/sheet1.xml");
    }

    /// <summary>
    /// Numbers and dates go in as numbers and dates. Written as text they would look right and
    /// refuse to sort or total, which is the main reason to offer Excel over CSV at all.
    /// </summary>
    [Fact]
    public void Numbers_And_Dates_Are_Not_Written_As_Text()
    {
        var sheet = ReadSheet(Write());

        // 1500.5 as a bare value, not an inline string.
        sheet.ShouldContain("<v>1500.5</v>");

        // 2026-03-31 is day 46112 in Excel's count from 1899-12-30.
        sheet.ShouldContain("<v>46112</v>");
    }

    [Fact]
    public void Text_Is_Escaped_So_The_File_Stays_Valid_Xml()
    {
        var bytes = SpreadsheetWriter.Write(
            "Sheet1",
            ["Name"],
            new List<IReadOnlyList<object>> { new object[] { "Smith & Sons <Ltd>" } }
        );

        var sheet = ReadSheet(bytes);

        sheet.ShouldContain("Smith &amp; Sons &lt;Ltd&gt;");
    }

    [Fact]
    public void A_Sheet_Name_Excel_Would_Reject_Is_Trimmed_Down()
    {
        var bytes = SpreadsheetWriter.Write("Customer/Vendor: 2026", ["A"], new List<IReadOnlyList<object>>());

        using var archive = new ZipArchive(new MemoryStream(bytes));
        using var reader = new StreamReader(archive.GetEntry("xl/workbook.xml")!.Open());
        var workbook = reader.ReadToEnd();

        workbook.ShouldContain("name=\"CustomerVendor 2026\"");
    }

    private static byte[] Write()
    {
        return SpreadsheetWriter.Write(
            "Customers",
            ["No.", "Name", "Balance", "Created"],
            new List<IReadOnlyList<object>>
            {
                new object[] { "C00010", "Adatum Corporation", 1500.5m, new DateTime(2026, 3, 31) },
            }
        );
    }

    private static string ReadSheet(byte[] bytes)
    {
        using var archive = new ZipArchive(new MemoryStream(bytes));
        using var reader = new StreamReader(archive.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        return reader.ReadToEnd();
    }
}
