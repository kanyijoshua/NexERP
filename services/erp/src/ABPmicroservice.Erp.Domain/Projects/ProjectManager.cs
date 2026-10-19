using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Projects;

/// <summary>Budget against actual of one task of a project.</summary>
public sealed record ProjectTaskStatistics(
    string TaskNo,
    string Description,
    ProjectTaskType TaskType,
    int Indentation,
    decimal BudgetCost,
    decimal BudgetPrice,
    decimal UsageCost,
    decimal UsagePrice,
    decimal InvoicedPrice
);

/// <summary>
/// Posts project usage and sales and works out work in progress. Resource time goes to the
/// resource ledger; items leave stock at their cost; G/L usage is paid from the line's balancing
/// account. Every usage writes a project ledger entry, which is what budgets are compared with,
/// invoices are made from and WIP is valued on.
/// </summary>
public class ProjectManager : DomainService
{
    public const string JournalSourceCode = "PROJJNL";
    public const string WipSourceCode = "PROJWIP";

    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectLedgerEntry, Guid> _entryRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();
    private IEntryNoGenerator EntryNo => Repo<IEntryNoGenerator>();
    private GenJnlPostLine GenJnlPostLine => Repo<GenJnlPostLine>();

    public ProjectManager(IRepository<Project, Guid> projectRepository, IRepository<ProjectLedgerEntry, Guid> entryRepository)
    {
        _projectRepository = projectRepository;
        _entryRepository = entryRepository;
    }

    public async Task<Project> GetProjectAsync(string projectNo)
    {
        var no = projectNo?.Trim().ToUpperInvariant();
        return await _projectRepository.FirstOrDefaultAsync(p => p.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Projects.ProjectNotFound).WithData("no", no ?? "");
    }

    public async Task<ProjectPostingGroup> GetPostingGroupAsync(Project project)
    {
        var code = project.PostingGroup;
        return (code == null ? null : await Repo<IRepository<ProjectPostingGroup, Guid>>().FirstOrDefaultAsync(g => g.Code == code))
            ?? throw new BusinessException(ErpErrorCodes.Projects.PostingGroupNotFound).WithData("no", project.No).WithData("postingGroup", code ?? "");
    }

    public async Task<Resource> GetResourceAsync(string resourceNo)
    {
        var no = resourceNo?.Trim().ToUpperInvariant();
        var resource = await Repo<IRepository<Resource, Guid>>().FirstOrDefaultAsync(r => r.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Projects.ResourceNotFound).WithData("no", no ?? "");
        if (resource.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Projects.ResourceBlocked).WithData("no", resource.No);
        }

        return resource;
    }

    /// <summary>Posts one usage line: the resource, item or G/L side and the project ledger entry.</summary>
    public async Task<ProjectLedgerEntry> PostUsageAsync(ProjectJournalLine line, GLPostingContext context)
    {
        var project = await _projectRepository.GetAsync((await GetProjectAsync(line.ProjectNo)).Id, includeDetails: true);
        project.EnsurePostable();
        var task = project.GetPostingTask(line.TaskNo);
        var group = await GetPostingGroupAsync(project);

        decimal unitCost, unitPrice;
        string description = line.Description;
        switch (line.Type)
        {
            case ProjectLineType.Resource:
            {
                var resource = await GetResourceAsync(line.No);
                unitCost = line.UnitCost ?? resource.UnitCost;
                unitPrice = line.UnitPrice ?? resource.UnitPrice;
                description ??= resource.Name;
                await Repo<IRepository<ResourceLedgerEntry, Guid>>().InsertAsync(new ResourceLedgerEntry(GuidGenerator.Create())
                {
                    EntryNo = await EntryNo.NextAsync(ErpSequenceNames.ResourceLedgerEntry),
                    ResourceNo = resource.No,
                    PostingDate = line.PostingDate,
                    DocumentNo = line.DocumentNo,
                    ProjectNo = project.No,
                    TaskNo = task.TaskNo,
                    Description = description,
                    Quantity = line.Quantity,
                    UnitCost = unitCost,
                    TotalCost = Math.Round(line.Quantity * unitCost, 2),
                    UnitPrice = unitPrice,
                    TotalPrice = Math.Round(line.Quantity * unitPrice, 2),
                    Chargeable = line.Chargeable,
                });
                break;
            }

            case ProjectLineType.Item:
            {
                var item = await Repo<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == line.No)
                    ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.No);
                description ??= item.Description;
                var entries = await Repo<ItemJnlPostLine>().PostAllAsync(new ItemPostingRequest
                {
                    ItemId = item.Id,
                    ItemNo = item.No,
                    PostingDate = line.PostingDate,
                    EntryType = ItemLedgerEntryType.NegativeAdjmt,
                    DocumentNo = line.DocumentNo,
                    Description = description,
                    Quantity = line.Quantity,
                    UnitCost = item.UnitCost,
                    LocationCode = line.LocationCode,
                });
                var cost = Math.Round(entries.Sum(e => Math.Abs(e.Quantity) * e.UnitCost), 2);
                unitCost = line.Quantity == 0m ? 0m : Math.Round(cost / line.Quantity, 5);
                unitPrice = line.UnitPrice ?? item.UnitPrice;

                if (cost != 0m)
                {
                    var inventoryAccount = await Repo<PostingSetupManager>().GetInventoryAccountAsync(item.InventoryPostingGroup, line.LocationCode);
                    await GLAsync(Required(group.ProjectCostsAccount, group, "ProjectCostsAccount"), cost, line, project, context);
                    await GLAsync(inventoryAccount, -cost, line, project, context);
                }
                break;
            }

            default:
            {
                unitCost = line.UnitCost ?? 0m;
                unitPrice = line.UnitPrice ?? 0m;
                var amount = Math.Round(line.Quantity * unitCost, 2);
                if (amount != 0m)
                {
                    if (line.BalAccountNo == null)
                    {
                        throw new BusinessException(ErpErrorCodes.Projects.BalAccountRequired).WithData("no", project.No).WithData("account", line.No);
                    }

                    await GLAsync(line.No, amount, line, project, context);
                    await GLAsync(line.BalAccountNo, -amount, line, project, context);
                }
                break;
            }
        }

        var entry = new ProjectLedgerEntry(GuidGenerator.Create())
        {
            EntryNo = await EntryNo.NextAsync(ErpSequenceNames.ProjectLedgerEntry),
            ProjectNo = project.No,
            TaskNo = task.TaskNo,
            PostingDate = line.PostingDate,
            DocumentNo = line.DocumentNo,
            EntryType = ProjectLedgerEntryType.Usage,
            Type = line.Type,
            No = line.No,
            Description = description,
            Quantity = line.Quantity,
            UnitCost = unitCost,
            TotalCost = Math.Round(line.Quantity * unitCost, 2),
            UnitPrice = unitPrice,
            TotalPrice = line.Chargeable ? Math.Round(line.Quantity * unitPrice, 2) : 0m,
            Chargeable = line.Chargeable,
            TransactionNo = context?.TransactionNo ?? 0,
        };
        await _entryRepository.InsertAsync(entry, autoSave: true);
        return entry;
    }

    /// <summary>
    /// Writes the project ledger entry of a posted sales or purchase line that names a project: a
    /// sale at the line's LCY amount, or a usage at its cost. The document posts the G/L itself.
    /// </summary>
    public async Task PostDocumentLineAsync(string projectNo, string taskNo, ProjectLedgerEntryType entryType, ProjectLineType type, string no, string description, decimal quantity, decimal amountLcy, DateTime postingDate, string documentNo, GLPostingContext context)
    {
        var project = await _projectRepository.GetAsync((await GetProjectAsync(projectNo)).Id, includeDetails: true);
        if (entryType == ProjectLedgerEntryType.Usage)
        {
            project.EnsurePostable();
        }

        var task = project.GetPostingTask(taskNo);
        var unit = quantity == 0m ? 0m : Math.Round(amountLcy / quantity, 5);
        await _entryRepository.InsertAsync(new ProjectLedgerEntry(GuidGenerator.Create())
        {
            EntryNo = await EntryNo.NextAsync(ErpSequenceNames.ProjectLedgerEntry),
            ProjectNo = project.No,
            TaskNo = task.TaskNo,
            PostingDate = postingDate.Date,
            DocumentNo = documentNo,
            EntryType = entryType,
            Type = type,
            No = no,
            Description = description,
            Quantity = quantity,
            UnitCost = entryType == ProjectLedgerEntryType.Usage ? unit : 0m,
            TotalCost = entryType == ProjectLedgerEntryType.Usage ? amountLcy : 0m,
            UnitPrice = entryType == ProjectLedgerEntryType.Sale ? unit : 0m,
            TotalPrice = entryType == ProjectLedgerEntryType.Sale ? amountLcy : 0m,
            Chargeable = entryType == ProjectLedgerEntryType.Sale,
            TransactionNo = context?.TransactionNo ?? 0,
        }, autoSave: true);
    }

    /// <summary>Budget and actual by task: planned cost and price, usage cost and price, invoiced.</summary>
    public async Task<List<ProjectTaskStatistics>> GetStatisticsAsync(string projectNo)
    {
        var project = await _projectRepository.GetAsync((await GetProjectAsync(projectNo)).Id, includeDetails: true);
        var planning = await Repo<IRepository<ProjectPlanningLine, Guid>>().GetListAsync(l => l.ProjectNo == project.No);
        var entries = await _entryRepository.GetListAsync(e => e.ProjectNo == project.No);

        return project.Tasks
            .OrderBy(t => t.TaskNo, StringComparer.Ordinal)
            .Select(t =>
            {
                var plans = planning.Where(l => l.TaskNo == t.TaskNo && l.IsBudget).ToList();
                var usage = entries.Where(e => e.TaskNo == t.TaskNo && e.EntryType == ProjectLedgerEntryType.Usage).ToList();
                return new ProjectTaskStatistics(
                    t.TaskNo,
                    t.Description,
                    t.TaskType,
                    t.Indentation,
                    plans.Sum(l => l.TotalCost),
                    plans.Sum(l => l.TotalPrice),
                    usage.Sum(e => e.TotalCost),
                    usage.Sum(e => e.TotalPrice),
                    entries.Where(e => e.TaskNo == t.TaskNo && e.EntryType == ProjectLedgerEntryType.Sale).Sum(e => e.TotalPrice)
                );
            })
            .ToList();
    }

    /// <summary>
    /// Values the project's work in progress at the date and posts it, after reversing the WIP of
    /// the last run. At cost, WIP is the usage cost; at sales value, it is the share of the budgeted
    /// price the cost so far has earned, less what has been invoiced. A completed project has no WIP,
    /// so its last WIP is only reversed. Returns the WIP now carried.
    /// </summary>
    public async Task<decimal> PostWipAsync(string projectNo, DateTime postingDate)
    {
        var project = await GetProjectAsync(projectNo);
        var group = await GetPostingGroupAsync(project);
        await Repo<GeneralLedgerSetupManager>().CheckPostingDateAsync(postingDate);

        var wipRepository = Repo<IRepository<ProjectWipEntry, Guid>>();
        var registerManager = Repo<GLRegisterManager>();
        var documentNo = $"WIP-{project.No}";
        var register = await registerManager.OpenAsync(postingDate, WipSourceCode, documentNo);
        var context = new GLPostingContext(register, WipSourceCode);

        foreach (var old in await wipRepository.GetListAsync(w => w.ProjectNo == project.No && !w.Reversed))
        {
            await PostWipGLAsync(old.AccountNo, old.BalAccountNo, -old.Amount, postingDate, documentNo, project, context);
            old.MarkReversed();
            await wipRepository.UpdateAsync(old);
        }

        var wip = 0m;
        if (project.Status != ProjectStatus.Completed)
        {
            var statistics = await GetStatisticsAsync(project.No);
            var usageCost = statistics.Sum(s => s.UsageCost);
            if (project.WipMethod == ProjectWipMethod.CostValue)
            {
                wip = usageCost;
                if (wip != 0m)
                {
                    var costs = new ProjectWipEntry(GuidGenerator.Create(), project.No, postingDate, ProjectWipEntryType.Costs, Required(group.WipCostsAccount, group, "WipCostsAccount"), Required(group.ProjectCostsAppliedAccount, group, "ProjectCostsAppliedAccount"), wip);
                    await PostWipGLAsync(costs.AccountNo, costs.BalAccountNo, wip, postingDate, documentNo, project, context);
                    await wipRepository.InsertAsync(costs);
                }
            }
            else
            {
                var budgetCost = statistics.Sum(s => s.BudgetCost);
                var budgetPrice = statistics.Sum(s => s.BudgetPrice);
                var invoiced = statistics.Sum(s => s.InvoicedPrice);
                var recognized = budgetCost == 0m ? 0m : Math.Round(budgetPrice * Math.Min(1m, usageCost / budgetCost), 2);
                wip = recognized - invoiced;
                if (wip > 0m)
                {
                    var sales = new ProjectWipEntry(GuidGenerator.Create(), project.No, postingDate, ProjectWipEntryType.Sales, Required(group.WipSalesAccount, group, "WipSalesAccount"), Required(group.RecognizedSalesAccount, group, "RecognizedSalesAccount"), wip);
                    await PostWipGLAsync(sales.AccountNo, sales.BalAccountNo, wip, postingDate, documentNo, project, context);
                    await wipRepository.InsertAsync(sales);
                }
                else
                {
                    wip = 0m;
                }
            }
        }

        await registerManager.CloseAsync(register);
        return wip;
    }

    private async Task PostWipGLAsync(string accountNo, string balAccountNo, decimal amount, DateTime date, string documentNo, Project project, GLPostingContext context)
    {
        await GenJnlPostLine.PostGLDirectAsync(accountNo, date, GLEntryDocumentType.None, documentNo, $"WIP {project.No}", amount, project.No, context: context);
        await GenJnlPostLine.PostGLDirectAsync(balAccountNo, date, GLEntryDocumentType.None, documentNo, $"WIP {project.No}", -amount, project.No, context: context);
    }

    private Task<GLEntry> GLAsync(string accountNo, decimal amount, ProjectJournalLine line, Project project, GLPostingContext context) =>
        GenJnlPostLine.PostGLDirectAsync(accountNo, line.PostingDate, GLEntryDocumentType.None, line.DocumentNo, line.Description ?? project.Description, amount, project.No, context: context);

    private static string Required(string accountNo, ProjectPostingGroup group, string what) =>
        accountNo.IsNullOrWhiteSpace()
            ? throw new BusinessException(ErpErrorCodes.Projects.PostingGroupAccountMissing).WithData("postingGroup", group.Code).WithData("account", what)
            : accountNo;
}

/// <summary>Posts project journal batches and moves approved time sheets into them.</summary>
public class ProjectJnlPostBatch : DomainService
{
    private readonly IRepository<ProjectJournalBatch, Guid> _batchRepository;
    private readonly IRepository<ProjectJournalLine, Guid> _lineRepository;
    private readonly ProjectManager _manager;

    public ProjectJnlPostBatch(IRepository<ProjectJournalBatch, Guid> batchRepository, IRepository<ProjectJournalLine, Guid> lineRepository, ProjectManager manager)
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _manager = manager;
    }

    public async Task<int> PostAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var lines = (await _lineRepository.GetListAsync(l => l.ProjectJournalBatchId == batchId)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Projects.NothingToPost).WithData("batch", batch.Name);
        }

        var glSetup = LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();
        foreach (var date in lines.Select(l => l.PostingDate).Distinct())
        {
            await glSetup.CheckPostingDateAsync(date);
        }

        var registers = LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
        var register = await registers.OpenAsync(lines[0].PostingDate, ProjectManager.JournalSourceCode, batch.Name);
        var context = new GLPostingContext(register, ProjectManager.JournalSourceCode);
        foreach (var line in lines)
        {
            await _manager.PostUsageAsync(line, context);
            await _lineRepository.DeleteAsync(line);
        }

        await registers.CloseAsync(register);
        return lines.Count;
    }

    /// <summary>
    /// Moves the approved, unposted lines of a time sheet into a project journal batch: one line per
    /// day with hours, dated that day. Returns the journal lines added.
    /// </summary>
    public async Task<int> TransferTimeSheetAsync(Guid timeSheetId, Guid batchId)
    {
        var sheets = LazyServiceProvider.LazyGetRequiredService<IRepository<TimeSheetHeader, Guid>>();
        var sheet = await sheets.GetAsync(timeSheetId, includeDetails: true);
        if (sheet.Status != TimeSheetStatus.Approved)
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetNotApproved).WithData("no", sheet.No);
        }

        await _batchRepository.GetAsync(batchId);
        var existing = await _lineRepository.GetListAsync(l => l.ProjectJournalBatchId == batchId);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var added = 0;

        foreach (var sheetLine in sheet.Lines.Where(l => !l.Posted).OrderBy(l => l.LineNo))
        {
            var days = sheetLine.Days;
            for (var day = 0; day < 7; day++)
            {
                if (days[day] == 0m)
                {
                    continue;
                }

                lineNo += 10000;
                var line = new ProjectJournalLine(GuidGenerator.Create(), batchId, lineNo);
                line.Set(sheet.StartingDate.AddDays(day), sheet.No, sheetLine.ProjectNo, sheetLine.TaskNo, ProjectLineType.Resource, sheet.ResourceNo, sheetLine.Description, days[day], null, null, sheetLine.Chargeable);
                line.LinkTimeSheet(sheetLine.Id);
                await _lineRepository.InsertAsync(line);
                added++;
            }

            sheetLine.MarkPosted();
        }

        await sheets.UpdateAsync(sheet, autoSave: true);
        return added;
    }
}
