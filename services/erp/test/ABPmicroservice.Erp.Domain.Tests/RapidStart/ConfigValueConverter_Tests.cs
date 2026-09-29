using System;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Inventory;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

public class ConfigValueConverter_Tests
{
    private static readonly ErpEntityRegistry Registry = new();

    private static ErpEntityField Field(string entity, string field) => Registry.Get(entity).GetField(field);

    [Theory]
    [InlineData("yes", true)]
    [InlineData("TRUE", true)]
    [InlineData("1", true)]
    [InlineData("x", true)]
    [InlineData("no", false)]
    [InlineData("0", false)]
    [InlineData("", false)] // blank on a non-nullable field is its default
    public void Reads_A_Boolean_The_Way_People_Write_One(string text, bool expected)
    {
        ConfigValueConverter.TryConvert(Field("Customer", "Blocked"), text, out var value).ShouldBeTrue();
        value.ShouldBe(expected);
    }

    [Fact]
    public void Refuses_Text_That_Is_Not_A_Boolean()
    {
        ConfigValueConverter.TryConvert(Field("Customer", "Blocked"), "perhaps", out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("1500.5", 1500.5)]
    [InlineData("1,500.50", 1500.5)]
    [InlineData("1.5E3", 1500)]
    public void Reads_Numbers_In_The_Invariant_Culture(string text, double expected)
    {
        ConfigValueConverter.TryConvert(Field("Customer", "CreditLimit"), text, out var value).ShouldBeTrue();
        value.ShouldBe((decimal)expected);
    }

    /// <summary>Excel keeps every number as a double, so a whole number comes back as "10.0".</summary>
    [Fact]
    public void Accepts_A_Whole_Number_Written_With_A_Decimal_Point()
    {
        ConfigValueConverter.TryConvert(Field("NoSeriesLine", "LineNo"), "10.0", out var value).ShouldBeTrue();
        value.ShouldBe(10);

        ConfigValueConverter.TryConvert(Field("NoSeriesLine", "LineNo"), "10.5", out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("Service", ItemType.Service)]
    [InlineData("service", ItemType.Service)]
    public void Reads_An_Enum_By_Name(string text, ItemType expected)
    {
        ConfigValueConverter.TryConvert(Field("Item", "Type"), text, out var value).ShouldBeTrue();
        value.ShouldBe(expected);
    }

    [Fact]
    public void Refuses_A_Number_That_Is_Not_One_Of_The_Enum_Values()
    {
        ConfigValueConverter.TryConvert(Field("Item", "Type"), "99", out _).ShouldBeFalse();
    }

    /// <summary>A date cell in a saved workbook holds the day count, not the text.</summary>
    [Theory]
    [InlineData("2026-03-31")]
    [InlineData("46112")]
    public void Reads_An_Iso_Date_Or_An_Excel_Serial(string text)
    {
        ConfigValueConverter.TryConvert(Field("NoSeriesLine", "StartingDate"), text, out var value).ShouldBeTrue();
        value.ShouldBe(new DateTime(2026, 3, 31));
    }

    [Fact]
    public void Blank_On_A_Nullable_Field_Is_No_Value()
    {
        ConfigValueConverter.TryConvert(Field("NoSeriesLine", "StartingDate"), " ", out var value).ShouldBeTrue();
        value.ShouldBeNull();
    }

    [Fact]
    public void Writes_Values_As_It_Reads_Them()
    {
        ConfigValueConverter.ToText(new DateTime(2026, 3, 31)).ShouldBe("2026-03-31");
        ConfigValueConverter.ToText(1500.5m).ShouldBe("1500.5");
        ConfigValueConverter.ToText(ItemType.Service).ShouldBe("Service");
        ConfigValueConverter.ToText(true).ShouldBe("true");
    }
}
