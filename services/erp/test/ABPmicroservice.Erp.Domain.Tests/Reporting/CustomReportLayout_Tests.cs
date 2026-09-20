using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class CustomReportLayout_Tests
{
    private static CustomReportLayout NewLayout(
        ReportLayoutType type = ReportLayoutType.Html,
        string template = "<p>{{Title}}</p>"
    )
    {
        return new CustomReportLayout(Guid.NewGuid(), "TrialBalance", "Our House Style", type, template);
    }

    [Fact]
    public void Keeps_The_Layout_It_Is_Given()
    {
        var layout = NewLayout();

        layout.ReportName.ShouldBe("TrialBalance");
        layout.TemplateContent.ShouldBe("<p>{{Title}}</p>");
        layout.IsDefault.ShouldBeFalse();
    }

    /// <summary>
    /// A broken layout is refused when it is uploaded, not discovered by whoever prints next.
    /// </summary>
    [Fact]
    public void Refuses_A_Layout_That_Cannot_Be_Rendered()
    {
        Should.Throw<BusinessException>(() => NewLayout(template: "{{Nonsense}}"))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateNotValid);
    }

    [Fact]
    public void Refuses_An_Empty_Layout()
    {
        Should.Throw<BusinessException>(() => NewLayout(template: " "))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateEmpty);
    }

    [Fact]
    public void Refuses_A_Layout_Longer_Than_The_Column()
    {
        var tooLong = "<p>" + new string('x', ErpDomainConsts.MaxLayoutTemplateLength) + "</p>";

        Should.Throw<BusinessException>(() => NewLayout(template: tooLong))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTemplateNotValid);
    }

    /// <summary>
    /// Word and Excel are in the model for parity with BC, so a layout of that type must be
    /// refused rather than accepted and quietly skipped at print time.
    /// </summary>
    [Theory]
    [InlineData(ReportLayoutType.Word)]
    [InlineData(ReportLayoutType.Excel)]
    public void Refuses_A_Format_Nothing_Can_Render_Yet(ReportLayoutType type)
    {
        Should.Throw<BusinessException>(() => NewLayout(type))
            .Code.ShouldBe(ErpErrorCodes.Reports.LayoutTypeNotRenderable);
    }

    [Fact]
    public void Can_Be_Made_The_One_In_Use()
    {
        var layout = NewLayout();

        layout.SetDefault(true);

        layout.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void Names_A_Report_Per_Kind_And_Per_Schedule()
    {
        ReportLayoutNames.For(ReportKind.TrialBalance).ShouldBe("TrialBalance");
        ReportLayoutNames.For(ReportKind.AccountSchedule, "BALANCE").ShouldBe("AccountSchedule:BALANCE");
        ReportLayoutNames.For(ReportKind.AccountSchedule).ShouldBe("AccountSchedule");

        ReportLayoutNames.ScheduleOf("AccountSchedule:BALANCE").ShouldBe("BALANCE");
        ReportLayoutNames.ScheduleOf("TrialBalance").ShouldBeNull();
    }
}
