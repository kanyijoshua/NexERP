using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// The .xlsx reader. It has to read what this system writes (inline strings, one sheet per table)
/// and what Excel writes when a user saves the file again (shared strings, sparse cells).
/// </summary>
public class SpreadsheetReader_Tests
{
    [Fact]
    public void Reads_Back_What_The_Writer_Wrote()
    {
        var bytes = SpreadsheetWriter.Write(
            [
                new SpreadsheetSheet("Customer", ["No", "Name", "CreditLimit", "Blocked"], [["C1", "Smith, Ltd.", 1500.5m, true]]),
                new SpreadsheetSheet("Vendor", ["No"], [["V1"], ["V2"]]),
            ]
        );

        var sheets = SpreadsheetReader.Read(bytes);

        sheets.Select(s => s.Name).ShouldBe(["Customer", "Vendor"]);
        sheets[0].Rows[0].ShouldBe(["No", "Name", "CreditLimit", "Blocked"]);
        sheets[0].Rows[1].ShouldBe(["C1", "Smith, Ltd.", "1500.5", "true"]);
        sheets[1].Rows.Count.ShouldBe(3);
    }

    /// <summary>Excel keeps text in a shared-string table and leaves empty cells out entirely.</summary>
    [Fact]
    public void Reads_Shared_Strings_And_Keeps_Gaps_In_Place()
    {
        var bytes = Workbook(
            sheet: """
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>
                  <row r="1"><c r="A1" t="s"><v>0</v></c><c r="C1" t="s"><v>1</v></c></row>
                  <row r="3"><c r="B3"><v>42</v></c></row>
                </sheetData></worksheet>
                """,
            sharedStrings: """
                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <si><t>Code</t></si>
                  <si><r><t>Descr</t></r><r><t>iption</t></r></si>
                </sst>
                """
        );

        var rows = SpreadsheetReader.Read(bytes).Single().Rows;

        rows[0].ShouldBe(["Code", "", "Description"]);
        rows[1].ShouldBeEmpty();
        rows[2].ShouldBe(["", "42"]);
    }

    [Fact]
    public void A_File_That_Is_Not_A_Workbook_Is_Refused()
    {
        var exception = Should.Throw<BusinessException>(() => SpreadsheetReader.Read(Encoding.UTF8.GetBytes("No,Name")));
        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.FileNotValid);
    }

    /// <summary>An uploaded file may declare a DTD to expand entities without end; it is refused, not expanded.</summary>
    [Fact]
    public void A_Document_Type_Definition_Is_Refused()
    {
        var bytes = Workbook(
            sheet: """
                <!DOCTYPE lol [<!ENTITY lol "lol">]>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>&lol;</t></is></c></row></sheetData></worksheet>
                """,
            sharedStrings: null
        );

        Should.Throw<BusinessException>(() => SpreadsheetReader.Read(bytes)).Code.ShouldBe(ErpErrorCodes.RapidStart.FileNotValid);
    }

    [Theory]
    [InlineData("A1", 0)]
    [InlineData("Z9", 25)]
    [InlineData("AA1", 26)]
    [InlineData("BA12", 52)]
    public void Turns_A_Cell_Reference_Into_A_Column(string reference, int expected)
    {
        SpreadsheetReader.ColumnIndex(reference).ShouldBe(expected);
    }

    private static byte[] Workbook(string sheet, string sharedStrings)
    {
        var parts = new Dictionary<string, string>
        {
            ["xl/workbook.xml"] = """
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Data" sheetId="1" r:id="rId7"/></sheets>
                </workbook>
                """,
            ["xl/_rels/workbook.xml.rels"] = """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId7" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/data.xml"/>
                </Relationships>
                """,
            ["xl/worksheets/data.xml"] = sheet,
        };

        if (sharedStrings != null)
        {
            parts["xl/sharedStrings.xml"] = sharedStrings;
        }

        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in parts)
            {
                using var writer = new StreamWriter(archive.CreateEntry(path).Open());
                writer.Write(content);
            }
        }

        return buffer.ToArray();
    }
}
