using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// CSV quoting, which is where naive exports go wrong: a customer called "Smith, Ltd." would
/// otherwise split into two columns and shift every field after it.
/// </summary>
public class DelimitedWriter_Tests
{
    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("Smith, Ltd.", "\"Smith, Ltd.\"")]
    [InlineData("say \"hello\"", "\"say \"\"hello\"\"\"")]
    [InlineData("two\nlines", "\"two\nlines\"")]
    [InlineData(null, "")]
    public void Quotes_Only_What_Has_To_Be_Quoted(string value, string expected)
    {
        DelimitedWriter.Escape(value, ',').ShouldBe(expected);
    }

    /// <summary>
    /// Values are written in the invariant culture. On a machine with a comma decimal separator
    /// the alternative would put a column break in the middle of every amount.
    /// </summary>
    [Fact]
    public void Numbers_And_Dates_Are_Written_Invariantly()
    {
        DelimitedWriter.Format(1234.56m).ShouldBe("1234.56");
        DelimitedWriter.Format(new DateTime(2026, 3, 31, 14, 5, 0)).ShouldBe("2026-03-31 14:05:00");
        DelimitedWriter.Format(true).ShouldBe("true");
        DelimitedWriter.Format(null).ShouldBe("");
    }

    [Fact]
    public void Writes_A_Header_Row_And_One_Row_Per_Record()
    {
        var csv = DelimitedWriter.Write(
            ["No.", "Name", "Balance"],
            new List<IReadOnlyList<object>>
            {
                new object[] { "C00010", "Adatum Corporation", 1500.5m },
                new object[] { "C00020", "Smith, Ltd.", -20m },
            }
        );

        csv.ShouldBe(
            "No.,Name,Balance\r\n" + "C00010,Adatum Corporation,1500.5\r\n" + "C00020,\"Smith, Ltd.\",-20\r\n"
        );
    }

    [Fact]
    public void A_Row_Shorter_Than_The_Header_Is_Padded_Rather_Than_Shifted()
    {
        var csv = DelimitedWriter.Write(["A", "B"], new List<IReadOnlyList<object>> { new object[] { "only" } });

        csv.ShouldBe("A,B\r\nonly,\r\n");
    }
}
