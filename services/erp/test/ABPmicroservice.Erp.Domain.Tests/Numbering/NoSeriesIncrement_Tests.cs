using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Numbering;

public class NoSeriesIncrement_Tests
{
    [Theory]
    [InlineData("1", 1, "2")]
    [InlineData("SI-00099", 1, "SI-00100")] // keeps its width
    [InlineData("99", 1, "100")] // grows only on overflow
    [InlineData("S-INV-0009", 1, "S-INV-0010")]
    [InlineData("1000", 10, "1010")] // Increment-by No.
    [InlineData("2026-INV009A", 1, "2026-INV010A")] // last digit group, text after it kept
    [InlineData("2026-0001", 1, "2026-0002")] // only the LAST group moves
    [InlineData("C00010", 5, "C00015")]
    [InlineData("99999999999999999999", 1, "100000000000000000000")] // beyond long
    public void Increments_The_Last_Digit_Group_Like_INCSTR(string no, int by, string expected)
    {
        NoSeriesIncrement.Increment(no, by).ShouldBe(expected);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("")]
    [InlineData(null)]
    public void Text_Without_Digits_Cannot_Be_Incremented(string no)
    {
        NoSeriesIncrement.Increment(no).ShouldBeNull();
    }

    [Theory]
    [InlineData("SI-0009", "SI-0010", -1)]
    [InlineData("SI-100", "SI-99", 1)] // by value, not by text ("100" < "99" as text)
    [InlineData("SI-0100", "SI-100", 0)]
    [InlineData("A-5", "B-1", -1)] // different prefix: plain text order
    public void Compares_By_Number_When_The_Surrounding_Text_Matches(string left, string right, int expectedSign)
    {
        System.Math.Sign(NoSeriesIncrement.Compare(left, right)).ShouldBe(expectedSign);
    }
}
