using System;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// The date formula is what moves a recurring journal on and what shifts a comparison column.
/// A formula that reads wrongly would post to the wrong period without anyone noticing, so the
/// Business Central forms are pinned down here.
/// </summary>
public class DateFormula_Tests
{
    [Theory]
    // Plain offsets.
    [InlineData("1D", "2026-01-15", "2026-01-16")]
    [InlineData("7D", "2026-01-15", "2026-01-22")]
    [InlineData("1W", "2026-01-15", "2026-01-22")]
    [InlineData("1M", "2026-01-15", "2026-02-15")]
    [InlineData("1Q", "2026-01-15", "2026-04-15")]
    [InlineData("1Y", "2026-01-15", "2027-01-15")]
    // A bare unit means one of it.
    [InlineData("M", "2026-01-15", "2026-02-15")]
    // Negative offsets go backwards.
    [InlineData("-1M", "2026-03-31", "2026-02-28")]
    [InlineData("-7D", "2026-01-08", "2026-01-01")]
    // The end of the current period.
    [InlineData("CM", "2026-02-10", "2026-02-28")]
    [InlineData("CQ", "2026-02-10", "2026-03-31")]
    [InlineData("CY", "2026-02-10", "2026-12-31")]
    // Terms apply left to right: a month on, then to the end of that month.
    [InlineData("1M+CM", "2026-01-15", "2026-02-28")]
    [InlineData("CM+1D", "2026-01-15", "2026-02-01")]
    public void Applies_The_Business_Central_Forms(string formula, string from, string expected)
    {
        var result = DateFormula.Parse(formula).Apply(DateTime.Parse(from));

        result.ShouldBe(DateTime.Parse(expected));
    }

    /// <summary>A month past the 31st lands on the last day of the shorter month, as BC does.</summary>
    [Fact]
    public void A_Month_From_The_End_Of_A_Long_Month_Lands_On_The_End_Of_A_Short_One()
    {
        DateFormula.Parse("1M").Apply(new DateTime(2026, 1, 31)).ShouldBe(new DateTime(2026, 2, 28));
    }

    [Fact]
    public void The_Week_Ends_On_Sunday()
    {
        // 2026-02-10 is a Tuesday.
        DateFormula.Parse("CW").Apply(new DateTime(2026, 2, 10)).ShouldBe(new DateTime(2026, 2, 15));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("X")]
    [InlineData("1X")]
    [InlineData("1M+")]
    [InlineData("C")]
    public void Refuses_Anything_It_Cannot_Read(string formula)
    {
        DateFormula.TryParse(formula, out _).ShouldBeFalse();
    }

    [Fact]
    public void Keeps_The_Text_It_Was_Given_In_Upper_Case()
    {
        DateFormula.Parse(" 1m+cm ").Text.ShouldBe("1M+CM");
    }
}
