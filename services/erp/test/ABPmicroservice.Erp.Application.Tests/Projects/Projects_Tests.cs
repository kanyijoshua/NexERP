using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Projects;

/// <summary>
/// Projects with tasks and plans, resources and their time sheets, the project journal, invoicing
/// from billable plans and work in progress. The seed has posting group PROJECTS (costs 5300,
/// sales 4100, WIP costs 1450 against 5310, WIP sales 1460 against 4110), series PROJ/RES/TS and
/// project journal batch DEFAULT; the sample customer C00010 and item 1000.
/// </summary>
public class Projects_Tests : ErpApplicationTestBase
{
    private readonly IProjectAppService _projects;
    private readonly IProjectTaskAppService _tasks;
    private readonly IProjectPlanningLineAppService _planning;
    private readonly IResourceAppService _resources;
    private readonly ITimeSheetAppService _timeSheets;
    private readonly IProjectJournalBatchAppService _batches;
    private readonly IProjectJournalLineAppService _journalLines;
    private readonly IRepository<ProjectLedgerEntry, Guid> _projectEntries;
    private readonly IRepository<ResourceLedgerEntry, Guid> _resourceEntries;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public Projects_Tests()
    {
        _projects = GetRequiredService<IProjectAppService>();
        _tasks = GetRequiredService<IProjectTaskAppService>();
        _planning = GetRequiredService<IProjectPlanningLineAppService>();
        _resources = GetRequiredService<IResourceAppService>();
        _timeSheets = GetRequiredService<ITimeSheetAppService>();
        _batches = GetRequiredService<IProjectJournalBatchAppService>();
        _journalLines = GetRequiredService<IProjectJournalLineAppService>();
        _projectEntries = GetRequiredService<IRepository<ProjectLedgerEntry, Guid>>();
        _resourceEntries = GetRequiredService<IRepository<ResourceLedgerEntry, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Approved_Time_Is_Posted_To_The_Project_And_Billed_From_The_Plan()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (project, resource) = await SetUpAsync("P-TS");
            await _planning.CreateAsync(new CreateUpdateProjectPlanningLineDto
            {
                ProjectNo = project.No, TaskNo = "100", LineType = ProjectPlanningLineType.BothBudgetAndBillable, Type = ProjectLineType.Resource, No = resource.No, Quantity = 20m, PlanningDate = new DateTime(2026, 3, 2),
            });

            var sheet = await _timeSheets.CreateAsync(new CreateUpdateTimeSheetDto
            {
                ResourceNo = resource.No,
                StartingDate = new DateTime(2026, 3, 4), // a Wednesday: the sheet starts on Monday 2 March
                Lines = [new TimeSheetLineInputDto { ProjectNo = project.No, TaskNo = "100", Description = "Design work", Days = [8m, 8m, 4m, 0m, 0m, 0m, 0m] }],
            });
            sheet.StartingDate.ShouldBe(new DateTime(2026, 3, 2));
            sheet.TotalHours.ShouldBe(20m);

            sheet = await _timeSheets.SubmitAsync(sheet.Id);
            var frozen = await Should.ThrowAsync<BusinessException>(() => _timeSheets.UpdateAsync(sheet.Id, new CreateUpdateTimeSheetDto { ResourceNo = resource.No, Lines = [] }));
            frozen.Code.ShouldBe(ErpErrorCodes.Projects.TimeSheetNotOpen);
            await _timeSheets.ApproveAsync(sheet.Id);

            var batch = (await _batches.GetListAsync(new GetCodeTableListInput { Filter = "DEFAULT" })).Items.Single();
            (await _timeSheets.TransferToJournalAsync(sheet.Id, new TransferTimeSheetInput { ProjectJournalBatchId = batch.Id })).ShouldBe(3);
            (await _batches.RunPostingAsync(batch.Id)).ShouldBe(3);

            (await _resourceEntries.GetListAsync(e => e.ResourceNo == resource.No)).Sum(e => e.Quantity).ShouldBe(20m);
            var usage = await _projectEntries.GetListAsync(e => e.ProjectNo == project.No && e.EntryType == ProjectLedgerEntryType.Usage);
            usage.Sum(e => e.TotalCost).ShouldBe(1000m); // 20 h at 50
            usage.Sum(e => e.TotalPrice).ShouldBe(2400m); // 20 h at 120

            var statistics = await _projects.GetStatisticsAsync(project.Id);
            var task = statistics.Single(s => s.TaskNo == "100");
            task.BudgetCost.ShouldBe(1000m);
            task.UsageCost.ShouldBe(1000m);

            // Bill the plan and post the invoice: a project sale on the project sales account.
            var invoice = await _projects.MakeSalesInvoiceAsync(project.Id, new ProjectPostingDateInput { PostingDate = new DateTime(2026, 3, 10) });
            invoice.LineCount.ShouldBe(1);
            var postedNo = (await GetRequiredService<ISalesDocumentAppService>().RunPostingAsync(invoice.InvoiceId)).PostedDocumentNo;

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "4100").ShouldBe(-2400m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            (await _projects.GetStatisticsAsync(project.Id)).Single(s => s.TaskNo == "100").InvoicedPrice.ShouldBe(2400m);

            var again = await Should.ThrowAsync<BusinessException>(() => _projects.MakeSalesInvoiceAsync(project.Id, new ProjectPostingDateInput()));
            again.Code.ShouldBe(ErpErrorCodes.Projects.NothingToInvoice);
        });
    }

    [Fact]
    public async Task Item_Usage_Takes_Stock_And_Charges_The_Project_At_Cost()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (project, _) = await SetUpAsync("P-ITEM");
            var item = await GetRequiredService<IRepository<Item, Guid>>().FirstAsync(i => i.No == "1000");
            await GetRequiredService<ItemJnlPostLine>().PostAsync(new ItemPostingRequest
            {
                ItemId = item.Id, ItemNo = item.No, PostingDate = new DateTime(2026, 3, 1), EntryType = ItemLedgerEntryType.PositiveAdjmt, DocumentNo = "P-ITEM-IN", Quantity = 5m, UnitCost = 100m,
            });
            var before = (await GetRequiredService<IRepository<ItemLedgerEntry, Guid>>().GetListAsync(e => e.ItemId == item.Id)).Sum(e => e.Quantity);

            var batch = (await _batches.GetListAsync(new GetCodeTableListInput { Filter = "DEFAULT" })).Items.Single();
            await _journalLines.CreateAsync(new CreateUpdateProjectJournalLineDto
            {
                ProjectJournalBatchId = batch.Id, PostingDate = new DateTime(2026, 3, 5), DocumentNo = "PJ-ITEM-1", ProjectNo = project.No, TaskNo = "100", Type = ProjectLineType.Item, No = "1000", Quantity = 2m,
            });
            await _batches.RunPostingAsync(batch.Id);

            var after = (await GetRequiredService<IRepository<ItemLedgerEntry, Guid>>().GetListAsync(e => e.ItemId == item.Id)).Sum(e => e.Quantity);
            after.ShouldBe(before - 2m);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "PJ-ITEM-1");
            Sum(entries, "5300").ShouldBeGreaterThan(0m);
            Sum(entries, "1400").ShouldBe(-Sum(entries, "5300"));
            (await _projectEntries.GetListAsync(e => e.DocumentNo == "PJ-ITEM-1")).Single().TotalCost.ShouldBe(Sum(entries, "5300"));
        });
    }

    [Fact]
    public async Task Wip_Is_Posted_At_Cost_And_Reversed_When_The_Project_Completes()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (project, resource) = await SetUpAsync("P-WIP");
            var batch = (await _batches.GetListAsync(new GetCodeTableListInput { Filter = "DEFAULT" })).Items.Single();
            await _journalLines.CreateAsync(new CreateUpdateProjectJournalLineDto
            {
                ProjectJournalBatchId = batch.Id, PostingDate = new DateTime(2026, 3, 5), DocumentNo = "PJ-WIP-1", ProjectNo = project.No, TaskNo = "100", Type = ProjectLineType.Resource, No = resource.No, Quantity = 10m,
            });
            await _batches.RunPostingAsync(batch.Id);

            (await _projects.CalculateWipAsync(project.Id, new ProjectPostingDateInput { PostingDate = new DateTime(2026, 3, 31) })).Wip.ShouldBe(500m);
            (await _glEntries.GetListAsync(e => e.GLAccountNo == "1450")).Sum(e => e.Amount).ShouldBe(500m);

            // Running it again reverses the last run first, so WIP is never counted twice.
            await _projects.CalculateWipAsync(project.Id, new ProjectPostingDateInput { PostingDate = new DateTime(2026, 3, 31) });
            (await _glEntries.GetListAsync(e => e.GLAccountNo == "1450")).Sum(e => e.Amount).ShouldBe(500m);

            await _projects.UpdateAsync(project.Id, new CreateUpdateProjectDto { Description = project.Description, CustomerNo = "C00010", Status = ProjectStatus.Completed, PostingGroup = "PROJECTS" });
            (await _projects.CalculateWipAsync(project.Id, new ProjectPostingDateInput { PostingDate = new DateTime(2026, 4, 30) })).Wip.ShouldBe(0m);
            (await _glEntries.GetListAsync(e => e.GLAccountNo == "1450")).Sum(e => e.Amount).ShouldBe(0m);
            (await _glEntries.GetListAsync(e => e.GLAccountNo == "5310")).Sum(e => e.Amount).ShouldBe(0m);

            // A completed project takes no more usage.
            await _journalLines.CreateAsync(new CreateUpdateProjectJournalLineDto
            {
                ProjectJournalBatchId = batch.Id, PostingDate = new DateTime(2026, 5, 5), DocumentNo = "PJ-WIP-2", ProjectNo = project.No, TaskNo = "100", Type = ProjectLineType.Resource, No = resource.No, Quantity = 1m,
            });
            var closed = await Should.ThrowAsync<BusinessException>(() => _batches.RunPostingAsync(batch.Id));
            closed.Code.ShouldBe(ErpErrorCodes.Projects.ProjectNotOpen);
        });
    }

    private async Task<(ProjectDto Project, ResourceDto Resource)> SetUpAsync(string no)
    {
        var project = await _projects.CreateAsync(new CreateUpdateProjectDto { No = no, Description = no + " project", CustomerNo = "C00010", Status = ProjectStatus.Open });
        await _tasks.CreateAsync(new CreateUpdateProjectTaskDto { ProjectId = project.Id, TaskNo = "000", Description = "Phases", TaskType = ProjectTaskType.Heading });
        await _tasks.CreateAsync(new CreateUpdateProjectTaskDto { ProjectId = project.Id, TaskNo = "100", Description = "Design", TaskType = ProjectTaskType.Posting, Indentation = 1 });
        var resource = await _resources.CreateAsync(new CreateUpdateResourceDto { No = "R-" + no, Name = "Designer " + no, UnitCost = 50m, UnitPrice = 120m });
        return (project, resource);
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
