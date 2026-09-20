using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class ReportTemplate_Tests
{
    private static ReportResult SampleResult()
    {
        var result = new ReportResult
        {
            Title = "Trial Balance",
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 12, 31),
        };

        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("amount", "Amount"));

        var row = result.AddRow();
        row.Values["accountNo"] = "1010";
        row.Values["amount"] = 1234.5m;
        row.Indentation = 2;

        var total = result.AddRow();
        total.Values["accountNo"] = "";
        total.Values["amount"] = 1234.5m;
        total.Bold = true;

        return result;
    }

    private static string Render(string template)
    {
        return ReportTemplate.Parse(template).Render(SampleResult(), new ReportRenderContext { CompanyName = "CRONUS" });
    }

    [Fact]
    public void Puts_The_Reports_Own_Values_In_Place()
    {
        var html = Render("<h1>{{Title}}</h1><p>{{CompanyName}}</p>");

        html.ShouldBe("<h1>Trial Balance</h1><p>CRONUS</p>");
    }

    [Fact]
    public void Repeats_A_Section_Once_Per_Column()
    {
        Render("{{#Columns}}<th>{{Header}}</th>{{/Columns}}").ShouldBe("<th>Account</th><th>Amount</th>");
    }

    [Fact]
    public void Repeats_A_Row_Section_Over_Every_Cell_In_Column_Order()
    {
        var html = Render("{{#Rows}}|{{#Cells}}{{Value}};{{/Cells}}{{/Rows}}");

        html.ShouldBe("|1010;1,234.50;|;1,234.50;");
    }

    /// <summary>A named cell is what lets a layout put one figure somewhere of its own.</summary>
    [Fact]
    public void Reads_One_Named_Cell_Of_A_Row()
    {
        Render("{{#Rows}}[{{Cell:accountNo}}]{{/Rows}}").ShouldBe("[1010][]");
    }

    [Fact]
    public void A_Named_Cell_Of_A_Column_This_Report_Does_Not_Have_Is_Blank()
    {
        Render("{{#Rows}}[{{Cell:notAColumn}}]{{/Rows}}").ShouldBe("[][]");
    }

    [Fact]
    public void Numbers_And_Dates_Are_Formatted_And_Text_Is_Left_Alone()
    {
        Render("{{#Rows}}{{#Cells}}{{Value}}|{{/Cells}}{{/Rows}}").ShouldContain("1,234.50");
        Render("{{FromDate}}").ShouldBe(new DateTime(2026, 1, 1).ToString("d"));
    }

    /// <summary>Bold and italic reach the layout as class names so the styling stays in it.</summary>
    [Fact]
    public void Offers_The_Rows_Formatting_As_Class_Names()
    {
        Render("{{#Rows}}<tr class=\"{{RowClass}}\" data-indent=\"{{Indent}}\">{{/Rows}}")
            .ShouldBe("<tr class=\"\" data-indent=\"2\"><tr class=\"total\" data-indent=\"0\">");
    }

    /// <summary>
    /// The markup is the layout author's and the data is only ever data, so a value that looks
    /// like markup is shown, not run.
    /// </summary>
    [Fact]
    public void Escapes_Every_Value_It_Substitutes()
    {
        var result = SampleResult();
        result.Title = "<script>alert(1)</script>";

        var html = ReportTemplate.Parse("{{Title}}").Render(result, new ReportRenderContext());

        html.ShouldBe("&lt;script&gt;alert(1)&lt;/script&gt;");
    }

    [Fact]
    public void Refuses_An_Empty_Layout()
    {
        Should.Throw<BusinessException>(() => ReportTemplate.Parse("   "))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateEmpty);
    }

    [Theory]
    [InlineData("{{Title")]
    [InlineData("{{}}")]
    [InlineData("{{Nonsense}}")]
    [InlineData("{{#Nonsense}}x{{/Nonsense}}")]
    [InlineData("{{#Rows}}x")]
    [InlineData("{{#Rows}}x{{/Columns}}")]
    [InlineData("{{/Rows}}")]
    [InlineData("{{#Columns}}{{Value}}{{/Columns}}")]
    [InlineData("{{Header}}")]
    public void Refuses_A_Layout_That_Would_Not_Render(string template)
    {
        Should.Throw<BusinessException>(() => ReportTemplate.Parse(template))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateNotValid);
    }

    /// <summary>The built-in layout is what a user starts from, so it has to be valid itself.</summary>
    [Fact]
    public void The_Built_In_Layout_Renders()
    {
        var html = Render(ReportLayoutTemplates.BuiltIn);

        html.ShouldContain("<title>Trial Balance</title>");
        html.ShouldContain("CRONUS");
        html.ShouldContain("<th>Account</th>");
        html.ShouldContain("1,234.50");
        html.ShouldContain("class=\"total indent-0\"");
    }
}
