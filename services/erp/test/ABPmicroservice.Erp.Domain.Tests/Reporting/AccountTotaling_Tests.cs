using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// The account filter decides which accounts a financial statement row adds up. Getting a range
/// boundary wrong would silently leave an account out of the balance sheet.
/// </summary>
public class AccountTotaling_Tests
{
    [Theory]
    [InlineData("1000..1999", "1000", true)]
    [InlineData("1000..1999", "1500", true)]
    [InlineData("1000..1999", "1999", true)]
    [InlineData("1000..1999", "0999", false)]
    [InlineData("1000..1999", "2000", false)]
    [InlineData("1000", "1000", true)]
    [InlineData("1000", "1001", false)]
    public void Matches_Single_Accounts_And_Ranges(string filter, string accountNo, bool expected)
    {
        AccountTotaling.Parse(filter).Matches(accountNo).ShouldBe(expected);
    }

    [Fact]
    public void Alternatives_Are_Separated_By_A_Pipe()
    {
        var filter = AccountTotaling.Parse("1000..1999|2100|4000..4999");

        filter.Matches("1500").ShouldBeTrue();
        filter.Matches("2100").ShouldBeTrue();
        filter.Matches("4200").ShouldBeTrue();
        filter.Matches("3000").ShouldBeFalse();
    }

    /// <summary>People type commas as often as pipes, so both are accepted.</summary>
    [Fact]
    public void Commas_Work_As_Well_As_Pipes()
    {
        var filter = AccountTotaling.Parse("1010, 1020");

        filter.Matches("1010").ShouldBeTrue();
        filter.Matches("1020").ShouldBeTrue();
        filter.Matches("1030").ShouldBeFalse();
    }

    [Fact]
    public void A_Range_Can_Be_Open_At_Either_End()
    {
        AccountTotaling.Parse("4000..").Matches("9999").ShouldBeTrue();
        AccountTotaling.Parse("4000..").Matches("3999").ShouldBeFalse();
        AccountTotaling.Parse("..1999").Matches("1000").ShouldBeTrue();
        AccountTotaling.Parse("..1999").Matches("2000").ShouldBeFalse();
    }

    /// <summary>
    /// Account numbers are compared as text, so a longer number under the upper bound is still
    /// inside the range: "1999" covers a sub-account "19990".
    /// </summary>
    [Fact]
    public void A_Longer_Number_Below_The_Upper_Bound_Is_Inside_The_Range()
    {
        AccountTotaling.Parse("1000..1999").Matches("19990").ShouldBeTrue();
    }

    [Fact]
    public void An_Empty_Filter_Matches_Nothing_And_All_Matches_Everything()
    {
        AccountTotaling.Parse("").IsEmpty.ShouldBeTrue();
        AccountTotaling.Parse("").Matches("1000").ShouldBeFalse();
        AccountTotaling.All.Matches("1000").ShouldBeTrue();
    }
}
