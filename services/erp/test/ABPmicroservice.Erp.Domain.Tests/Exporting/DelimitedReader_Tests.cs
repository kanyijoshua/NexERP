using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

public class DelimitedReader_Tests
{
    /// <summary>What the writer escapes, the reader must take back as one field.</summary>
    [Fact]
    public void Reads_Quoted_Fields_With_Separators_Quotes_And_Line_Breaks()
    {
        var text = DelimitedWriter.Write(["No", "Name"], [["C1", "Smith, \"The\" Ltd.\r\nBranch"]]);

        var rows = DelimitedReader.Read(text);

        rows.Count.ShouldBe(2);
        rows[1].ShouldBe(["C1", "Smith, \"The\" Ltd.\r\nBranch"]);
    }

    [Theory]
    [InlineData("No;Name\nC1;Adatum", ';')]
    [InlineData("No\tName\nC1\tAdatum", '\t')]
    [InlineData("No,Name\nC1,Adatum", ',')]
    [InlineData("\"A;B\",Name\nC1,Adatum", ',')] // a separator inside quotes does not count
    public void Detects_The_Separator_From_The_Header_Line(string text, char expected)
    {
        DelimitedReader.Detect(text).ShouldBe(expected);
        DelimitedReader.Read(text)[1].ShouldBe(["C1", "Adatum"]);
    }

    [Fact]
    public void Skips_Blank_Lines_And_The_Byte_Order_Mark()
    {
        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("No,Name\r\n\r\nC1,Café\r\n")).ToArray();

        var rows = DelimitedReader.Read(bytes);

        rows.Count.ShouldBe(2);
        rows[0][0].ShouldBe("No");
        rows[1].ShouldBe(["C1", "Café"]);
    }

    /// <summary>Excel on a Western Windows machine saves CSV in Windows-1252, not UTF-8.</summary>
    [Fact]
    public void Falls_Back_To_Latin1_When_The_File_Is_Not_Utf8()
    {
        var bytes = Encoding.Latin1.GetBytes("No,Name\nC1,Café");

        DelimitedReader.Read(bytes)[1][1].ShouldBe("Café");
    }
}
