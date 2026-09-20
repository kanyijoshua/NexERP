using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class ReportLayoutAppService_Tests : ErpApplicationTestBase
{
    private const string Report = "TrialBalance";
    private const string Template = "<html><body><h1>{{Title}}</h1>{{#Rows}}<p>{{Cell:name}}</p>{{/Rows}}</body></html>";

    private readonly IReportLayoutAppService _layouts;
    private readonly IFinancialReportAppService _reports;

    public ReportLayoutAppService_Tests()
    {
        _layouts = GetRequiredService<IReportLayoutAppService>();
        _reports = GetRequiredService<IFinancialReportAppService>();
    }

    private Task<ReportLayoutDto> CreateAsync(string layoutName = "House Style", string template = Template)
    {
        return InCompanyAsync(
            DefaultCompanyName,
            () =>
                _layouts.CreateAsync(
                    new CreateUpdateReportLayoutDto
                    {
                        ReportName = Report,
                        LayoutName = layoutName,
                        LayoutType = ReportLayoutType.Html,
                        TemplateContent = template,
                        Description = "Ours",
                    }
                )
        );
    }

    [Fact]
    public async Task Keeps_A_Layout_And_Gives_It_Back_With_Its_Body()
    {
        var created = await CreateAsync();

        var detail = await InCompanyAsync(DefaultCompanyName, () => _layouts.GetAsync(created.Id));

        detail.LayoutName.ShouldBe("House Style");
        detail.LayoutType.ShouldBe(ReportLayoutType.Html);
        detail.TemplateContent.ShouldBe(Template);
    }

    /// <summary>
    /// A list is for choosing between layouts, so it carries no bodies; only the one being edited
    /// is fetched in full.
    /// </summary>
    [Fact]
    public async Task Lists_The_Layouts_Of_One_Report()
    {
        await CreateAsync("First");
        await CreateAsync("Second");

        var list = await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.GetListAsync(new GetReportLayoutsInput { ReportName = Report })
        );

        list.Items.Select(l => l.LayoutName).ShouldContain("First");
        list.Items.Select(l => l.LayoutName).ShouldContain("Second");
        list.Items.ShouldAllBe(l => l.ReportName == Report);
    }

    [Fact]
    public async Task Refuses_Two_Layouts_Of_One_Report_With_The_Same_Name()
    {
        await CreateAsync("Duplicate");

        var exception = await Should.ThrowAsync<BusinessException>(() => CreateAsync("Duplicate"));

        exception.Code.ShouldBe(ErpErrorCodes.Reports.LayoutNameAlreadyExists);
    }

    [Fact]
    public async Task Refuses_A_Layout_That_Would_Not_Render()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() => CreateAsync("Broken", "{{Nonsense}}"));

        exception.Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateNotValid);
    }

    [Fact]
    public async Task Refuses_To_Move_A_Layout_To_Another_Report()
    {
        var created = await CreateAsync();

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            InCompanyAsync(
                DefaultCompanyName,
                () =>
                    _layouts.UpdateAsync(
                        created.Id,
                        new CreateUpdateReportLayoutDto
                        {
                            ReportName = "BalanceSheet",
                            LayoutName = created.LayoutName,
                            LayoutType = ReportLayoutType.Html,
                            TemplateContent = Template,
                        }
                    )
            )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Reports.LayoutBelongsToAnotherReport);
    }

    /// <summary>
    /// The point of the whole feature: once a layout is chosen, the printed report comes out
    /// through it instead of the built-in one.
    /// </summary>
    [Fact]
    public async Task A_Printed_Report_Comes_Out_Through_The_Chosen_Layout()
    {
        var created = await CreateAsync("Marker", "<html><body>UNMISTAKABLE {{Title}}</body></html>");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.SetDefaultAsync(new SetDefaultReportLayoutInput { ReportName = Report, LayoutId = created.Id })
        );

        var html = await PrintAsync(ReportKind.TrialBalance);

        html.ShouldContain("UNMISTAKABLE Trial Balance");
    }

    [Fact]
    public async Task A_Report_With_No_Layout_Prints_Through_The_Built_In_One()
    {
        var html = await PrintAsync(ReportKind.BalanceSheet);

        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("Printed ");
    }

    /// <summary>
    /// Deleting the layout in use must not leave a selection pointing at nothing, or the report
    /// would quietly print through the built-in layout with no sign of why.
    /// </summary>
    [Fact]
    public async Task Deleting_The_Layout_In_Use_Clears_The_Choice()
    {
        var created = await CreateAsync("Temporary");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.SetDefaultAsync(new SetDefaultReportLayoutInput { ReportName = Report, LayoutId = created.Id })
        );

        await InCompanyAsync(DefaultCompanyName, () => _layouts.DeleteAsync(created.Id));

        var list = await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.GetListAsync(new GetReportLayoutsInput { ReportName = Report })
        );
        list.Items.ShouldBeEmpty();

        (await PrintAsync(ReportKind.TrialBalance)).ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task Only_One_Layout_Of_A_Report_Is_In_Use_At_A_Time()
    {
        var first = await CreateAsync("First");
        var second = await CreateAsync("Second");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.SetDefaultAsync(new SetDefaultReportLayoutInput { ReportName = Report, LayoutId = first.Id })
        );
        await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.SetDefaultAsync(new SetDefaultReportLayoutInput { ReportName = Report, LayoutId = second.Id })
        );

        var list = await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.GetListAsync(new GetReportLayoutsInput { ReportName = Report })
        );

        list.Items.Count(l => l.IsDefault).ShouldBe(1);
        list.Items.Single(l => l.IsDefault).LayoutName.ShouldBe("Second");
    }

    [Fact]
    public async Task Offers_Every_Report_A_Layout_Can_Belong_To()
    {
        var names = await InCompanyAsync(DefaultCompanyName, () => _layouts.GetReportNamesAsync());

        names.Items.Select(n => n.Name).ShouldContain("TrialBalance");
        names.Items.Select(n => n.Name).ShouldContain("AgedPayables");

        // Each seeded account schedule prints as a report of its own.
        names.Items.Select(n => n.Name).ShouldContain("AccountSchedule:BALANCE");
    }

    [Fact]
    public async Task Hands_Out_The_Built_In_Layout_To_Start_From()
    {
        var template = await InCompanyAsync(DefaultCompanyName, () => _layouts.GetBuiltInTemplateAsync());

        template.ShouldBe(ReportLayoutTemplates.BuiltIn);
        ReportTemplate.Validate(template);
    }

    /// <summary>A layout can be checked before it is saved, and before the books have anything in them.</summary>
    [Fact]
    public async Task Previews_A_Layout_Against_Sample_Figures()
    {
        var html = await InCompanyAsync(
            DefaultCompanyName,
            () => _layouts.RunPreviewAsync(new PreviewReportLayoutInput { TemplateContent = Template })
        );

        html.ShouldContain("<h1>Trial Balance</h1>");
        html.ShouldContain("Accounts Receivable");
    }

    [Fact]
    public async Task A_Preview_Of_A_Broken_Layout_Says_So()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            InCompanyAsync(
                DefaultCompanyName,
                () => _layouts.RunPreviewAsync(new PreviewReportLayoutInput { TemplateContent = "{{#Rows}}oops" })
            )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateNotValid);
    }

    private async Task<string> PrintAsync(ReportKind report)
    {
        var stream = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _reports.RunExportAsync(
                    new ReportExportInput
                    {
                        Report = report,
                        Format = ExportFormat.Html,
                        FromDate = new DateTime(2026, 1, 1),
                        ToDate = new DateTime(2026, 12, 31),
                    }
                )
        );

        using var reader = new StreamReader(stream.GetStream());
        return await reader.ReadToEndAsync();
    }
}
