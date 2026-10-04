using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Querying;

/// <summary>The filter pane's conditions and filter expressions, over plain objects.</summary>
public class DynamicFilter_Tests
{
    private enum Kind
    {
        Posting = 0,
        Heading = 1,
        Total = 2,
    }

    private sealed class Row
    {
        public string No { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public int? Rank { get; set; }
        public bool Blocked { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Kind Type { get; set; }
    }

    private static readonly DateTime Today = new(2026, 9, 30); // a Wednesday

    private static readonly List<Row> Rows =
    [
        new() { No = "1000", Name = "Adatum Corporation", Balance = 500m, Rank = 1, PostingDate = new DateTime(2026, 9, 30), DueDate = new DateTime(2026, 10, 30), Type = Kind.Posting },
        new() { No = "1500", Name = "Trey Research", Balance = -20m, Rank = null, PostingDate = new DateTime(2026, 9, 28), Type = Kind.Heading },
        new() { No = "2000", Name = null, Balance = 0m, Rank = 3, Blocked = true, PostingDate = new DateTime(2026, 8, 1), Type = Kind.Total },
        new() { No = "3000", Name = "School of Fine Art", Balance = 1200m, Rank = 4, PostingDate = new DateTime(2025, 12, 31), Type = Kind.Posting },
    ];

    private static string[] Match(string json) =>
        Rows.AsQueryable().ApplyDynamicFilter(json, Today).Select(r => r.No).ToArray();

    private static string One(string field, string op, string value = null, string valueTo = null, string logic = "and") =>
        $$"""{"logic":"{{logic}}","conditions":[{"field":"{{field}}","operator":"{{op}}","value":{{Json(value)}},"valueTo":{{Json(valueTo)}}}]}""";

    private static string Json(string value) => value == null ? "null" : $"\"{value}\"";

    [Fact]
    public void Blank_Changes_Nothing() => Match(null).Length.ShouldBe(4);

    [Theory]
    [InlineData("name", "contains", "RESEARCH", "1500")]
    [InlineData("name", "startsWith", "ada", "1000")]
    [InlineData("name", "endsWith", "art", "3000")]
    [InlineData("name", "isEmpty", null, "2000")]
    [InlineData("balance", "gt", "0", "1000,3000")]
    [InlineData("balance", "lte", "0", "1500,2000")]
    [InlineData("rank", "isEmpty", null, "1500")]
    [InlineData("blocked", "isTrue", null, "2000")]
    [InlineData("type", "equals", "0", "1000,3000")]
    [InlineData("type", "equals", "Total", "2000")]
    [InlineData("postingDate", "today", null, "1000")]
    [InlineData("postingDate", "thisWeek", null, "1000,1500")]
    [InlineData("postingDate", "thisYear", null, "1000,1500,2000")]
    [InlineData("postingDate", "equals", "2026-09-28", "1500")]
    [InlineData("postingDate", "before", "2026-01-01", "3000")]
    [InlineData("dueDate", "isNotEmpty", null, "1000")]
    public void Each_Operator_Filters_By_The_Fields_Type(string field, string op, string value, string expected)
    {
        Match(One(field, op, value)).ShouldBe(expected.Split(','));
    }

    [Fact]
    public void Between_Includes_Both_Ends_And_Either_May_Be_Open()
    {
        Match(One("balance", "between", "0", "500")).ShouldBe(["1000", "2000"]);
        Match(One("postingDate", "between", "2026-09-01", null)).ShouldBe(["1000", "1500"]);
    }

    [Theory]
    [InlineData("no", "1000..2000", "1000,1500,2000")]
    [InlineData("no", "..1500|3000", "1000,1500,3000")]
    [InlineData("no", "<>1000&<>2000", "1500,3000")]
    [InlineData("name", "*research*", "1500")]
    [InlineData("name", "@adatum*", "1000")]
    [InlineData("name", "s*art", "3000")]
    [InlineData("name", "''", "2000")]
    [InlineData("balance", ">=500", "1000,3000")]
    [InlineData("balance", "<0|1200", "1500,3000")]
    [InlineData("postingDate", "t", "1000")]
    [InlineData("postingDate", "2026-09-01..t", "1000,1500")]
    public void Business_Central_Filter_Expressions(string field, string expression, string expected)
    {
        Match(One(field, "expression", expression)).ShouldBe(expected.Split(','));
    }

    [Fact]
    public void Conditions_Combine_With_And_Or_Or()
    {
        const string both = """{"logic":"and","conditions":[{"field":"balance","operator":"gt","value":0},{"field":"no","operator":"expression","value":"<2000"}]}""";
        const string either = """{"logic":"or","conditions":[{"field":"blocked","operator":"isTrue"},{"field":"no","operator":"equals","value":"3000"}]}""";

        Match(both).ShouldBe(["1000"]);
        Match(either).ShouldBe(["2000", "3000"]);
    }

    [Fact]
    public void Unknown_Fields_Bad_Values_And_Wrong_Operators_Are_Refused()
    {
        Should.Throw<BusinessException>(() => Match(One("secret", "equals", "x"))).Code.ShouldBe(ErpErrorCodes.Querying.FieldNotFilterable);
        Should.Throw<BusinessException>(() => Match(One("balance", "equals", "lots"))).Code.ShouldBe(ErpErrorCodes.Querying.InvalidValue);
        Should.Throw<BusinessException>(() => Match(One("balance", "contains", "5"))).Code.ShouldBe(ErpErrorCodes.Querying.OperatorNotSupported);
        Should.Throw<BusinessException>(() => Match("{not json")).Code.ShouldBe(ErpErrorCodes.Querying.InvalidFilter);
    }

    private class CodedBase
    {
        public CodedBase(string code) => Code = code;

        public string Code { get; private set; }
    }

    private sealed class Coded : CodedBase
    {
        public Coded(string code)
            : base(code) { }

        public string Display => "#" + Code;
    }

    [Fact]
    public void Inherited_Columns_Filter_But_Computed_Values_Do_Not()
    {
        var rows = new[] { new Coded("30D"), new Coded("COD"), new Coded("CM") }.AsQueryable();

        rows.ApplyDynamicFilter(One("code", "expression", "*D"), Today).Select(r => r.Code).ShouldBe(["30D", "COD"]);
        Should.Throw<BusinessException>(() => rows.ApplyDynamicFilter(One("display", "equals", "#CM"), Today).ToList())
            .Code.ShouldBe(ErpErrorCodes.Querying.FieldNotFilterable);
    }

    [Fact]
    public void An_Alias_Maps_A_List_Field_To_Its_Property()
    {
        var aliases = new Dictionary<string, string> { ["partyNo"] = "No" };
        Rows.AsQueryable().ApplyDynamicFilter(One("partyNo", "equals", "1500"), Today, aliases).Single().No.ShouldBe("1500");
    }
}
