using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class RdlcLayoutConverter_Tests : ErpApplicationTestBase
{
    private static readonly IReadOnlyList<ReportColumnDefinition> MemberColumns =
    [
        new("no", "Member No.", ReportColumnKind.Text),
        new("name", "Name", ReportColumnKind.Text),
        new("employee", "Employee Contribution"),
        new("employer", "Employer Contribution"),
        new("total", "Fund Value"),
    ];

    private static string Rdlc(string body, string header = "", string prolog = "")
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>{prolog}
            <Report xmlns="http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition">
              <ReportSections><ReportSection>
                <Body><ReportItems>{body}</ReportItems></Body>
                <Page>
                  <PageHeader><ReportItems>{header}</ReportItems></PageHeader>
                  <PageHeight>21cm</PageHeight><PageWidth>29.7cm</PageWidth>
                  <LeftMargin>2cm</LeftMargin><RightMargin>1cm</RightMargin><TopMargin>1.5cm</TopMargin><BottomMargin>1.5cm</BottomMargin>
                </Page>
              </ReportSection></ReportSections>
            </Report>
            """;
    }

    private static string Cell(string value, string style = "") =>
        $"<TablixCell><CellContents><Textbox Name=\"t\"><Paragraphs><Paragraph><TextRuns><TextRun><Value>{value}</Value><Style>{style}</Style></TextRun></TextRuns><Style><TextAlign>Right</TextAlign></Style></Paragraph></Paragraphs></Textbox></CellContents></TablixCell>";

    private static string Table(params string[][] rows)
    {
        var columns = string.Concat(rows[0].Select(_ => "<TablixColumn><Width>3cm</Width></TablixColumn>"));
        var body = string.Concat(rows.Select(row => $"<TablixRow><Height>0.5cm</Height><TablixCells>{string.Concat(row)}</TablixCells></TablixRow>"));
        return $"<Tablix Name=\"Table1\"><TablixBody><TablixColumns>{columns}</TablixColumns><TablixRows>{body}</TablixRows></TablixBody></Tablix>";
    }

    private static readonly string MemberBalancesRdlc = Rdlc(
        Table(
            [Cell("PF No.", "<FontWeight>Bold</FontWeight><Color>#003366</Color>"), Cell("=First(Fields!Vendor_NameCaption.Value)"), Cell("Employee"), Cell("Loan Balance")],
            [Cell("=Fields!Vendor__No__.Value", "<FontFamily>Times New Roman</FontFamily><FontSize>8pt</FontSize>"), Cell("=Fields!Vendor_Name.Value"), Cell("=Fields!EmployeeContr.Value"), Cell("=Fields!LoanBalance.Value")],
            [Cell(""), Cell("Total"), Cell("=Sum(Fields!EmployeeContr.Value)"), Cell("")]
        ),
        "<Textbox Name=\"Title\"><Paragraphs><Paragraph><TextRuns><TextRun><Value>MEMBER BALANCES</Value><Style><FontSize>14pt</FontSize><FontWeight>Bold</FontWeight></Style></TextRun></TextRuns></Paragraph></Paragraphs></Textbox>"
    );

    [Fact]
    public void A_List_Layout_Keeps_Its_Look_And_Its_Columns_Are_Fitted_To_The_Report()
    {
        var conversion = RdlcLayoutConverter.Convert(MemberBalancesRdlc, MemberColumns);

        // "PF No." and the name are matched by caption and field name; the employee contribution
        // by the abbreviation in its field name. The loan balance has no counterpart.
        conversion.MatchedColumns.ShouldBe(3);
        conversion.Columns.Where(c => c.RdlcField != null).Select(c => c.ReportColumnKey).ShouldBe(["no", "name", "employee", null]);
        conversion.Columns.First().Caption.ShouldBe("PF No.");
        conversion.Notes.ShouldContain(n => n.Contains("Loan Balance"));

        // The report's own columns the layout did not show come last.
        conversion.Columns.Where(c => c.RdlcField == null).Select(c => c.ReportColumnKey).ShouldBe(["employer", "total"]);

        conversion.Template.ShouldContain("size: 29.7cm 21cm");
        conversion.Template.ShouldContain("margin: 1.5cm 1cm 1.5cm 2cm");
        conversion.Template.ShouldContain("font-family: Times New Roman, sans-serif");
        conversion.Template.ShouldContain("<h1>MEMBER BALANCES</h1>");
        conversion.Template.ShouldContain("<th>PF No.</th>");
        conversion.Template.ShouldContain("{{Cell:employee}}");
        conversion.Template.ShouldNotContain("Loan Balance");
    }

    [Fact]
    public void The_Converted_Layout_Renders_The_Report()
    {
        var conversion = RdlcLayoutConverter.Convert(MemberBalancesRdlc, MemberColumns);

        var result = new ReportResult { Title = "Member Balances", FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 12, 31) };
        foreach (var column in MemberColumns)
        {
            result.Columns.Add(column);
        }

        var row = result.AddRow();
        row.Values["no"] = "M000010";
        row.Values["name"] = "Wanjiku <Mwangi>";
        row.Values["employee"] = 5000m;
        row.Values["total"] = 15000m;

        var html = ReportTemplate.Parse(conversion.Template).Render(result, new ReportRenderContext { CompanyName = "CRONUS", PrintedOn = new DateTime(2026, 10, 3) });

        html.ShouldContain("M000010");
        html.ShouldContain("Wanjiku &lt;Mwangi&gt;");
        html.ShouldContain("5,000.00");
        html.ShouldContain("CRONUS");
    }

    [Fact]
    public void A_Layout_With_No_Detail_Table_Lends_Its_Page_And_Fonts_To_The_Reports_Own_Columns()
    {
        var form = Rdlc(Table([Cell("Member Details"), Cell(":")], [Cell("PF No."), Cell("=Fields!No.Value")]));

        var conversion = RdlcLayoutConverter.Convert(form, MemberColumns);

        conversion.MatchedColumns.ShouldBe(0);
        conversion.Template.ShouldContain("{{#Columns}}");
        conversion.Template.ShouldContain("size: 29.7cm 21cm");
        ReportTemplate.Validate(conversion.Template);
    }

    [Fact]
    public void What_A_File_Says_Is_Text_And_Never_Markup_Or_A_Placeholder()
    {
        var hostile = Rdlc(
            Table(
                [Cell("&lt;script&gt;alert(1)&lt;/script&gt;"), Cell("{{CompanyName}}")],
                [Cell("=Fields!Vendor__No__.Value", "<FontFamily>x; } body { background: url(javascript:alert(1))</FontFamily><Color>expression(alert(1))</Color>"), Cell("=Fields!Vendor_Name.Value")]
            ),
            "<Textbox Name=\"Title\"><Paragraphs><Paragraph><TextRuns><TextRun><Value>&lt;img src=x onerror=alert(1)&gt; Title</Value></TextRun></TextRuns></Paragraph></Paragraphs></Textbox>"
        );

        var template = RdlcLayoutConverter.Convert(hostile, MemberColumns).Template;

        template.ShouldNotContain("<script>");
        template.ShouldNotContain("<img");
        template.ShouldNotContain("javascript:");
        template.ShouldNotContain("expression(");
        template.ShouldContain("&lt;script&gt;");
        // A caption that looks like a placeholder is printed as it is written, not filled in.
        template.ShouldContain("{ {CompanyName} }");
        ReportTemplate.Validate(template);
    }

    [Fact]
    public void A_File_That_Is_Not_An_Rdlc_Or_Declares_Entities_Is_Refused()
    {
        Should.Throw<BusinessException>(() => RdlcLayoutConverter.Convert("not xml", MemberColumns)).Code.ShouldBe(ErpErrorCodes.Reports.RdlcNotValid);
        Should.Throw<BusinessException>(() => RdlcLayoutConverter.Convert("<Invoice />", MemberColumns)).Code.ShouldBe(ErpErrorCodes.Reports.RdlcNotValid);

        var withEntity = Rdlc(Table([Cell("&x;")]), prolog: "<!DOCTYPE Report [<!ENTITY x SYSTEM \"file:///c:/windows/win.ini\">]>");
        Should.Throw<BusinessException>(() => RdlcLayoutConverter.Convert(withEntity, MemberColumns)).Code.ShouldBe(ErpErrorCodes.Reports.RdlcNotValid);
    }

    [Fact]
    public async Task Every_Bundled_Layout_Is_Seeded_For_Its_Report_And_Renders()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var layouts = await GetRequiredService<IRepository<CustomReportLayout, Guid>>().GetListAsync(l => l.BuiltIn);
            var resolver = GetRequiredService<ReportColumnsResolver>();

            foreach (var bundled in BundledReportLayouts.All)
            {
                var layout = layouts.SingleOrDefault(l => l.ReportName == bundled.ReportName && l.LayoutName == bundled.LayoutName);
                layout.ShouldNotBeNull(bundled.FileName);
                layout.IsDefault.ShouldBeFalse();

                // The layout was fitted to a report that exists and has columns.
                (await resolver.GetAsync(bundled.ReportName)).ShouldNotBeEmpty(bundled.ReportName);
                ReportTemplate.Validate(layout.TemplateContent);
            }
        });
    }

    [Theory]
    [InlineData("MemberBalances.rdlc", 3)]
    [InlineData("MemberListingDC.rdlc", 8)]
    [InlineData("ExpectedRetirees.rdlc", 8)]
    [InlineData("EmployeeReport.rdl", 3)]
    [InlineData("BankStatementReport.rdl", 5)]
    public async Task The_Bundled_List_Layouts_Fit_Their_Reports(string fileName, int atLeast)
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var bundled = BundledReportLayouts.All.Single(l => l.FileName == fileName);
            var columns = await GetRequiredService<ReportColumnsResolver>().GetAsync(bundled.ReportName);

            var conversion = RdlcLayoutConverter.Convert(BundledReportLayouts.Read(bundled), columns);

            conversion.MatchedColumns.ShouldBeGreaterThanOrEqualTo(atLeast, string.Join("; ", conversion.Columns.Select(c => $"{c.RdlcField}->{c.ReportColumnKey}")));
            conversion.Template.ShouldContain("{{Cell:");
        });
    }

    [Fact]
    public async Task An_Uploaded_Rdlc_Becomes_A_Layout_Of_The_Report()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var service = GetRequiredService<IReportLayoutAppService>();
            var reportName = ReportLayoutNames.ForStandardReport("MemberBalances");

            (await service.GetReportNamesAsync()).Items.ShouldContain(n => n.Name == reportName);

            var imported = await service.ImportRdlcAsync(new ImportRdlcLayoutInput
            {
                ReportName = reportName,
                LayoutName = "Trustee pack",
                ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(MemberBalancesRdlc)),
                SetAsDefault = true,
            });

            imported.Layout.LayoutName.ShouldBe("Trustee pack");
            imported.MatchedColumns.ShouldBeGreaterThanOrEqualTo(2);
            imported.Notes.ShouldNotBeEmpty();

            // The report now prints through it.
            var template = await GetRequiredService<ReportLayoutRenderer>().GetTemplateAsync(reportName);
            template.ShouldContain("<h1>MEMBER BALANCES</h1>");

            var notRdlc = await Should.ThrowAsync<BusinessException>(() => service.ImportRdlcAsync(new ImportRdlcLayoutInput
            {
                ReportName = reportName,
                LayoutName = "Broken",
                ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("<html></html>")),
            }));
            notRdlc.Code.ShouldBe(ErpErrorCodes.Reports.RdlcNotValid);
        });
    }
}
