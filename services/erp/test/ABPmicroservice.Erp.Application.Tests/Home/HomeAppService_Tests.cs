using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Modules;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Home;

public class HomeAppService_Tests : ErpApplicationTestBase
{
    private readonly IHomeAppService _home;
    private readonly IModuleAppService _modules;
    private readonly IGeneralJournalAppService _journals;

    public HomeAppService_Tests()
    {
        _home = GetRequiredService<IHomeAppService>();
        _modules = GetRequiredService<IModuleAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
    }

    private Task<HomeSummaryDto> SummaryAsync()
    {
        return InCompanyAsync(DefaultCompanyName, () => _home.GetSummaryAsync());
    }

    [Fact]
    public async Task Says_Which_Company_Is_Being_Worked_In()
    {
        var summary = await SummaryAsync();

        summary.CompanyName.ShouldBe(DefaultCompanyName);
    }

    [Fact]
    public async Task Offers_The_Apps_That_Are_On()
    {
        var summary = await SummaryAsync();

        summary.Apps.ShouldNotBeEmpty();
        summary.Apps.Select(a => a.Code).ShouldContain(ErpModuleRegistry.Sales);
        summary.Apps.ShouldAllBe(a => a.Enabled);
    }

    /// <summary>
    /// A cue is a figure with somewhere to go: one that pointed nowhere would be a dead end on the
    /// page a user opens on.
    /// </summary>
    [Fact]
    public async Task Every_Cue_Has_A_Name_And_Somewhere_To_Go()
    {
        var summary = await SummaryAsync();

        summary.Cues.ShouldNotBeEmpty();
        summary.Cues.ShouldAllBe(c => !string.IsNullOrWhiteSpace(c.Route));
        summary.Cues.ShouldAllBe(c => !c.DisplayName.StartsWith("Cue:"));
    }

    /// <summary>The figures are counted from the books, not decoration on the page.</summary>
    [Fact]
    public async Task A_Cue_Counts_What_Is_Actually_There()
    {
        var before = await CueValueAsync("UnpostedJournalLines");

        await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                var batch = await _journals.CreateBatchAsync(
                    new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = "HOMECUE" }
                );

                await _journals.CreateLineAsync(
                    new CreateUpdateGenJournalLineDto
                    {
                        GenJournalBatchId = batch.Id,
                        PostingDate = new DateTime(2026, 3, 1),
                        DocumentNo = "HC-001",
                        AccountType = GenJournalAccountType.GLAccount,
                        AccountNo = "1010",
                        Description = "Home page cue",
                        Amount = 100m,
                        BalAccountType = GenJournalAccountType.GLAccount,
                        BalAccountNo = "4000",
                    }
                );
            }
        );

        (await CueValueAsync("UnpostedJournalLines")).ShouldBe(before + 1);
    }

    /// <summary>
    /// The home page must never advertise a figure from a module that is switched off, or it
    /// would invite the user into a part of the system that is closed.
    /// </summary>
    [Fact]
    public async Task A_Switched_Off_Module_Takes_Its_Cues_With_It()
    {
        var before = await SummaryAsync();
        before.Cues.Select(c => c.Key).ShouldContain("OpenSalesInvoices");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.Sales, Enabled = false })
        );

        var after = await SummaryAsync();
        after.Cues.Select(c => c.Key).ShouldNotContain("OpenSalesInvoices");
        after.Cues.Select(c => c.Key).ShouldNotContain("OverdueReceivables");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.Sales, Enabled = true })
        );
    }

    [Fact]
    public async Task An_Amount_Cue_Is_Marked_As_Money()
    {
        var summary = await SummaryAsync();

        summary.Cues.Single(c => c.Key == "OverdueReceivables").IsAmount.ShouldBeTrue();
        summary.Cues.Single(c => c.Key == "OpenSalesInvoices").IsAmount.ShouldBeFalse();
    }

    private async Task<decimal> CueValueAsync(string key)
    {
        var summary = await SummaryAsync();

        return summary.Cues.SingleOrDefault(c => c.Key == key)?.Value ?? 0m;
    }
}
