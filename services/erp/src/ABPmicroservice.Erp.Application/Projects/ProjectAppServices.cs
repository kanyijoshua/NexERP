using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Projects;

[Authorize(ErpPermissions.ProjectsSetup.Default)]
public class ProjectsSetupAppService : ErpAppService, IProjectsSetupAppService
{
    private readonly IRepository<ProjectsSetup, Guid> _repository;

    public ProjectsSetupAppService(IRepository<ProjectsSetup, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ProjectsSetupDto> GetAsync() => Map(await GetOrCreateAsync());

    [Authorize(ErpPermissions.ProjectsSetup.Update)]
    public async Task<ProjectsSetupDto> UpdateAsync(ProjectsSetupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<NoSeriesCodeValidator>().EnsureExistAsync(input.ProjectNos, input.ResourceNos, input.TimeSheetNos);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ProjectPostingGroup>(input.DefaultPostingGroup);
        var setup = await GetOrCreateAsync();
        setup.Set(input.ProjectNos, input.ResourceNos, input.TimeSheetNos, input.DefaultPostingGroup);
        await _repository.UpdateAsync(setup, autoSave: true);
        return Map(setup);
    }

    private async Task<ProjectsSetup> GetOrCreateAsync() =>
        await _repository.FirstOrDefaultAsync() ?? await _repository.InsertAsync(new ProjectsSetup(GuidGenerator.Create()), autoSave: true);

    private static ProjectsSetupDto Map(ProjectsSetup s) => new()
    {
        ProjectNos = s.ProjectNos,
        ResourceNos = s.ResourceNos,
        TimeSheetNos = s.TimeSheetNos,
        DefaultPostingGroup = s.DefaultPostingGroup,
    };
}

public class ProjectPostingGroupAppService : CodeTableAppServiceBase<ProjectPostingGroup, ProjectPostingGroupDto, CreateUpdateProjectPostingGroupDto>, IProjectPostingGroupAppService
{
    public ProjectPostingGroupAppService(IRepository<ProjectPostingGroup, Guid> repository)
        : base(repository, ErpPermissions.ProjectsSetup.Default) { }

    protected override ProjectPostingGroup NewEntity(Guid id, CreateUpdateProjectPostingGroupDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(ProjectPostingGroup entity, CreateUpdateProjectPostingGroupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(
            input.ProjectCostsAccount, input.ProjectSalesAccount, input.WipCostsAccount, input.ProjectCostsAppliedAccount, input.WipSalesAccount, input.RecognizedSalesAccount);
        entity.SetAccounts(input.ProjectCostsAccount, input.ProjectSalesAccount, input.WipCostsAccount, input.ProjectCostsAppliedAccount, input.WipSalesAccount, input.RecognizedSalesAccount);
    }
}

/// <summary>Shared by the project services: setup, number series and unique numbers.</summary>
internal static class ProjectNumbering
{
    public static async Task<ProjectsSetup> SetupAsync(Volo.Abp.DependencyInjection.IAbpLazyServiceProvider services) =>
        await services.LazyGetRequiredService<IRepository<ProjectsSetup, Guid>>().FirstOrDefaultAsync();
}

[Authorize(ErpPermissions.Projects.Default)]
public class ResourceAppService
    : ErpCrudAppService<Resource, ResourceDto, Guid, GetCodeTableListInput, CreateUpdateResourceDto, CreateUpdateResourceDto>,
        IResourceAppService
{
    public ResourceAppService(IRepository<Resource, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.ProjectsSetup.Create;
        UpdatePolicyName = ErpPermissions.ProjectsSetup.Update;
        DeletePolicyName = ErpPermissions.ProjectsSetup.Delete;
    }

    public override async Task<ResourceDto> CreateAsync(CreateUpdateResourceDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await ProjectNumbering.SetupAsync(LazyServiceProvider);
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup?.ResourceNos, input.No, Clock.Now);
        if (await Repository.AnyAsync(r => r.No == no.ToUpper()))
        {
            throw new BusinessException(ErpErrorCodes.Projects.AlreadyExists).WithData("no", no);
        }

        var resource = new Resource(GuidGenerator.Create(), no, input.Name, input.Type);
        await ApplyAsync(resource, input);
        await Repository.InsertAsync(resource, autoSave: true);
        return await MapToGetOutputDtoAsync(resource);
    }

    public override async Task<ResourceDto> UpdateAsync(Guid id, CreateUpdateResourceDto input)
    {
        await CheckUpdatePolicyAsync();
        var resource = await GetEntityByIdAsync(id);
        resource.SetName(input.Name);
        await ApplyAsync(resource, input);
        await Repository.UpdateAsync(resource, autoSave: true);
        return await MapToGetOutputDtoAsync(resource);
    }

    private async Task ApplyAsync(Resource resource, CreateUpdateResourceDto input)
    {
        if (!input.EmployeeNo.IsNullOrWhiteSpace())
        {
            await LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>().EnsureNoExistsAsync<HumanResources.Employee>(input.EmployeeNo);
        }

        resource.Set(input.Type, input.EmployeeNo, input.UnitOfMeasureCode, input.UnitCost, input.UnitPrice, input.CapacityPerDay, input.Blocked);
    }

    protected override async Task<IQueryable<Resource>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), r => r.No.ToLower().Contains(filter) || r.Name.ToLower().Contains(filter));
    }

    protected override IQueryable<Resource> ApplyDefaultSorting(IQueryable<Resource> query) => query.OrderBy(r => r.No);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectAppService
    : ErpCrudAppService<Project, ProjectDto, Guid, GetCodeTableListInput, CreateUpdateProjectDto, CreateUpdateProjectDto>,
        IProjectAppService
{
    private ProjectManager Manager => LazyServiceProvider.LazyGetRequiredService<ProjectManager>();

    public ProjectAppService(IRepository<Project, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.Projects.Create;
        UpdatePolicyName = ErpPermissions.Projects.Update;
        DeletePolicyName = ErpPermissions.Projects.Delete;
    }

    public override async Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await ProjectNumbering.SetupAsync(LazyServiceProvider);
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup?.ProjectNos, input.No, Clock.Now);
        if (await Repository.AnyAsync(p => p.No == no.ToUpper()))
        {
            throw new BusinessException(ErpErrorCodes.Projects.AlreadyExists).WithData("no", no);
        }

        var project = new Project(GuidGenerator.Create(), no, input.Description);
        await ApplyAsync(project, input, setup?.DefaultPostingGroup);
        await Repository.InsertAsync(project, autoSave: true);
        return await MapToGetOutputDtoAsync(project);
    }

    public override async Task<ProjectDto> UpdateAsync(Guid id, CreateUpdateProjectDto input)
    {
        await CheckUpdatePolicyAsync();
        var project = await GetEntityByIdAsync(id);
        project.SetDescription(input.Description);
        await ApplyAsync(project, input, null);
        await Repository.UpdateAsync(project, autoSave: true);
        return await MapToGetOutputDtoAsync(project);
    }

    public async Task<List<ProjectTaskStatisticsDto>> GetStatisticsAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var project = await GetEntityByIdAsync(id);
        return (await Manager.GetStatisticsAsync(project.No)).Select(s => new ProjectTaskStatisticsDto
        {
            TaskNo = s.TaskNo, Description = s.Description, TaskType = s.TaskType, Indentation = s.Indentation,
            BudgetCost = s.BudgetCost, BudgetPrice = s.BudgetPrice, UsageCost = s.UsageCost, UsagePrice = s.UsagePrice, InvoicedPrice = s.InvoicedPrice,
        }).ToList();
    }

    [Authorize(ErpPermissions.Projects.Post)]
    public async Task<ProjectWipResultDto> CalculateWipAsync(Guid id, ProjectPostingDateInput input)
    {
        var project = await GetEntityByIdAsync(id);
        return new ProjectWipResultDto { Wip = await Manager.PostWipAsync(project.No, input.PostingDate == default ? Clock.Now.Date : input.PostingDate) };
    }

    [Authorize(ErpPermissions.Projects.Post)]
    public async Task<ProjectInvoiceResultDto> MakeSalesInvoiceAsync(Guid id, ProjectPostingDateInput input)
    {
        var project = await GetEntityByIdAsync(id);
        if (project.CustomerId == null)
        {
            throw new BusinessException(ErpErrorCodes.Projects.NoCustomer).WithData("no", project.No);
        }

        var group = await Manager.GetPostingGroupAsync(project);
        var planning = LazyServiceProvider.LazyGetRequiredService<IRepository<ProjectPlanningLine, Guid>>();
        var lines = (await planning.GetListAsync(l => l.ProjectNo == project.No)).Where(l => l.QtyToInvoice != 0m).OrderBy(l => l.TaskNo).ThenBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Projects.NothingToInvoice).WithData("no", project.No);
        }

        // Project sales are billed on the posting group's sales account; each line remembers its task.
        var invoice = await LazyServiceProvider.LazyGetRequiredService<ISalesDocumentAppService>().CreateAsync(new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            CustomerId = project.CustomerId.Value,
            PostingDate = input.PostingDate == default ? Clock.Now.Date : input.PostingDate,
            ExternalDocumentNo = project.No,
            Lines = lines.Select(l => new SalesLineInputDto
            {
                Type = DocumentLineType.GLAccount,
                No = group.ProjectSalesAccount,
                Description = l.Description ?? l.No,
                Quantity = l.QtyToInvoice,
                UnitPrice = l.UnitPrice,
                ProjectNo = project.No,
                ProjectTaskNo = l.TaskNo,
            }).ToList(),
        });

        foreach (var line in lines)
        {
            line.TransferToInvoice(line.QtyToInvoice, invoice.No);
            await planning.UpdateAsync(line);
        }

        return new ProjectInvoiceResultDto { InvoiceId = invoice.Id, InvoiceNo = invoice.No, LineCount = lines.Count };
    }

    private async Task ApplyAsync(Project project, CreateUpdateProjectDto input, string defaultPostingGroup)
    {
        Customer customer = null;
        if (!input.CustomerNo.IsNullOrWhiteSpace())
        {
            var no = input.CustomerNo.Trim();
            customer = await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().FirstOrDefaultAsync(c => c.No == no)
                ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", no);
        }

        var postingGroup = input.PostingGroup.IsNullOrWhiteSpace() ? defaultPostingGroup ?? project.PostingGroup : input.PostingGroup;
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ProjectPostingGroup>(postingGroup);
        project.SetCustomer(customer?.Id, customer?.No, customer?.Name);
        project.Set(input.Status, postingGroup, input.WipMethod, input.StartingDate, input.EndingDate, input.PersonResponsible);
    }

    protected override async Task<IQueryable<Project>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), p => p.No.ToLower().Contains(filter) || p.Description.ToLower().Contains(filter) || (p.CustomerName != null && p.CustomerName.ToLower().Contains(filter)));
    }

    protected override IQueryable<Project> ApplyDefaultSorting(IQueryable<Project> query) => query.OrderBy(p => p.No);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectTaskAppService
    : ErpCrudAppService<ProjectTask, ProjectTaskDto, Guid, GetProjectTaskListInput, CreateUpdateProjectTaskDto, CreateUpdateProjectTaskDto>,
        IProjectTaskAppService
{
    private readonly IRepository<Project, Guid> _projects;

    public ProjectTaskAppService(IRepository<ProjectTask, Guid> repository, IRepository<Project, Guid> projects)
        : base(repository)
    {
        _projects = projects;
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.Projects.Update;
        UpdatePolicyName = ErpPermissions.Projects.Update;
        DeletePolicyName = ErpPermissions.Projects.Update;
    }

    public override async Task<ProjectTaskDto> CreateAsync(CreateUpdateProjectTaskDto input)
    {
        await CheckCreatePolicyAsync();
        var project = await _projects.GetAsync(input.ProjectId, includeDetails: true);
        if (project.FindTask(input.TaskNo) != null)
        {
            throw new BusinessException(ErpErrorCodes.Projects.AlreadyExists).WithData("no", input.TaskNo);
        }

        var task = new ProjectTask(GuidGenerator.Create(), project.Id, input.TaskNo, input.Description, input.TaskType, input.Indentation);
        await Repository.InsertAsync(task, autoSave: true);
        return await MapToGetOutputDtoAsync(task);
    }

    public override async Task<ProjectTaskDto> UpdateAsync(Guid id, CreateUpdateProjectTaskDto input)
    {
        await CheckUpdatePolicyAsync();
        var task = await GetEntityByIdAsync(id);
        task.Update(input.TaskNo, input.Description, input.TaskType, input.Indentation);
        await Repository.UpdateAsync(task, autoSave: true);
        return await MapToGetOutputDtoAsync(task);
    }

    protected override async Task<IQueryable<ProjectTask>> CreateFilteredQueryAsync(GetProjectTaskListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ProjectId.HasValue, t => t.ProjectId == input.ProjectId.Value);
    }

    protected override IQueryable<ProjectTask> ApplyDefaultSorting(IQueryable<ProjectTask> query) => query.OrderBy(t => t.TaskNo);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectPlanningLineAppService
    : ErpCrudAppService<ProjectPlanningLine, ProjectPlanningLineDto, Guid, GetProjectPlanningLineListInput, CreateUpdateProjectPlanningLineDto, CreateUpdateProjectPlanningLineDto>,
        IProjectPlanningLineAppService
{
    public ProjectPlanningLineAppService(IRepository<ProjectPlanningLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.Projects.Update;
        UpdatePolicyName = ErpPermissions.Projects.Update;
        DeletePolicyName = ErpPermissions.Projects.Update;
    }

    public override async Task<ProjectPlanningLineDto> CreateAsync(CreateUpdateProjectPlanningLineDto input)
    {
        await CheckCreatePolicyAsync();
        var project = await LazyServiceProvider.LazyGetRequiredService<ProjectManager>().GetProjectAsync(input.ProjectNo);
        var lines = await Repository.GetListAsync(l => l.ProjectNo == project.No);
        var line = new ProjectPlanningLine(GuidGenerator.Create(), project.No, input.TaskNo.Trim().ToUpperInvariant(), (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000);
        await ApplyAsync(line, project, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ProjectPlanningLineDto> UpdateAsync(Guid id, CreateUpdateProjectPlanningLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var project = await LazyServiceProvider.LazyGetRequiredService<ProjectManager>().GetProjectAsync(line.ProjectNo);
        await ApplyAsync(line, project, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    private async Task ApplyAsync(ProjectPlanningLine line, Project project, CreateUpdateProjectPlanningLineDto input)
    {
        var full = await LazyServiceProvider.LazyGetRequiredService<IRepository<Project, Guid>>().GetAsync(project.Id, includeDetails: true);
        full.GetPostingTask(input.TaskNo);

        decimal cost = input.UnitCost ?? 0m, price = input.UnitPrice ?? 0m;
        var description = input.Description;
        if (input.Type == ProjectLineType.Resource)
        {
            var resource = await LazyServiceProvider.LazyGetRequiredService<ProjectManager>().GetResourceAsync(input.No);
            cost = input.UnitCost ?? resource.UnitCost;
            price = input.UnitPrice ?? resource.UnitPrice;
            description ??= resource.Name;
        }
        else if (input.Type == ProjectLineType.Item)
        {
            var no = input.No.Trim();
            var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no);
            cost = input.UnitCost ?? item.UnitCost;
            price = input.UnitPrice ?? item.UnitPrice;
            description ??= item.Description;
        }

        line.Set(input.TaskNo, input.LineType, input.Type, input.No, description, input.PlanningDate == default ? Clock.Now.Date : input.PlanningDate, input.Quantity, cost, price);
    }

    protected override async Task<IQueryable<ProjectPlanningLine>> CreateFilteredQueryAsync(GetProjectPlanningLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var no = input.ProjectNo?.Trim().ToUpperInvariant();
        return query.WhereIf(!no.IsNullOrEmpty(), l => l.ProjectNo == no);
    }

    protected override IQueryable<ProjectPlanningLine> ApplyDefaultSorting(IQueryable<ProjectPlanningLine> query) => query.OrderBy(l => l.TaskNo).ThenBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectJournalBatchAppService
    : ErpCrudAppService<ProjectJournalBatch, ProjectJournalBatchDto, Guid, GetCodeTableListInput, CreateUpdateProjectJournalBatchDto, CreateUpdateProjectJournalBatchDto>,
        IProjectJournalBatchAppService
{
    public ProjectJournalBatchAppService(IRepository<ProjectJournalBatch, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.Projects.Update;
        UpdatePolicyName = ErpPermissions.Projects.Update;
        DeletePolicyName = ErpPermissions.Projects.Update;
    }

    public override async Task<ProjectJournalBatchDto> CreateAsync(CreateUpdateProjectJournalBatchDto input)
    {
        await CheckCreatePolicyAsync();
        var name = input.Name.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(b => b.Name == name))
        {
            throw new BusinessException(ErpErrorCodes.Projects.AlreadyExists).WithData("no", name);
        }

        var batch = new ProjectJournalBatch(GuidGenerator.Create(), input.Name, input.Description);
        await Repository.InsertAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    public override async Task<ProjectJournalBatchDto> UpdateAsync(Guid id, CreateUpdateProjectJournalBatchDto input)
    {
        await CheckUpdatePolicyAsync();
        var batch = await GetEntityByIdAsync(id);
        batch.SetName(input.Name);
        batch.SetDescription(input.Description);
        await Repository.UpdateAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    [Authorize(ErpPermissions.Projects.Post)]
    public Task<int> RunPostingAsync(Guid id) => LazyServiceProvider.LazyGetRequiredService<ProjectJnlPostBatch>().PostAsync(id);

    protected override async Task<IQueryable<ProjectJournalBatch>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), b => b.Name.ToLower().Contains(filter));
    }

    protected override IQueryable<ProjectJournalBatch> ApplyDefaultSorting(IQueryable<ProjectJournalBatch> query) => query.OrderBy(b => b.Name);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectJournalLineAppService
    : ErpCrudAppService<ProjectJournalLine, ProjectJournalLineDto, Guid, GetProjectJournalLineListInput, CreateUpdateProjectJournalLineDto, CreateUpdateProjectJournalLineDto>,
        IProjectJournalLineAppService
{
    public ProjectJournalLineAppService(IRepository<ProjectJournalLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
        CreatePolicyName = ErpPermissions.Projects.Update;
        UpdatePolicyName = ErpPermissions.Projects.Update;
        DeletePolicyName = ErpPermissions.Projects.Update;
    }

    public override async Task<ProjectJournalLineDto> CreateAsync(CreateUpdateProjectJournalLineDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<IRepository<ProjectJournalBatch, Guid>>().GetAsync(input.ProjectJournalBatchId);
        var lines = await Repository.GetListAsync(l => l.ProjectJournalBatchId == input.ProjectJournalBatchId);
        var line = new ProjectJournalLine(GuidGenerator.Create(), input.ProjectJournalBatchId, (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000);
        await ApplyAsync(line, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ProjectJournalLineDto> UpdateAsync(Guid id, CreateUpdateProjectJournalLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    private async Task ApplyAsync(ProjectJournalLine line, CreateUpdateProjectJournalLineDto input)
    {
        var project = await LazyServiceProvider.LazyGetRequiredService<ProjectManager>().GetProjectAsync(input.ProjectNo);
        var full = await LazyServiceProvider.LazyGetRequiredService<IRepository<Project, Guid>>().GetAsync(project.Id, includeDetails: true);
        full.GetPostingTask(input.TaskNo);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Location>(input.LocationCode);
        line.Set(input.PostingDate == default ? Clock.Now.Date : input.PostingDate, input.DocumentNo, project.No, input.TaskNo, input.Type, input.No, input.Description, input.Quantity, input.UnitCost, input.UnitPrice, input.Chargeable);
        line.SetPosting(input.LocationCode, input.BalAccountNo);
    }

    protected override async Task<IQueryable<ProjectJournalLine>> CreateFilteredQueryAsync(GetProjectJournalLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ProjectJournalBatchId.HasValue, l => l.ProjectJournalBatchId == input.ProjectJournalBatchId.Value);
    }

    protected override IQueryable<ProjectJournalLine> ApplyDefaultSorting(IQueryable<ProjectJournalLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ProjectLedgerEntryAppService : ErpReadOnlyAppService<ProjectLedgerEntry, ProjectLedgerEntryDto, Guid, GetProjectLedgerListInput>, IProjectLedgerEntryAppService
{
    public ProjectLedgerEntryAppService(IRepository<ProjectLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
    }

    protected override async Task<IQueryable<ProjectLedgerEntry>> CreateFilteredQueryAsync(GetProjectLedgerListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ProjectNo.IsNullOrWhiteSpace(), e => e.ProjectNo == input.ProjectNo)
            .WhereIf(!filter.IsNullOrEmpty(), e => e.ProjectNo.ToLower().Contains(filter) || (e.DocumentNo != null && e.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<ProjectLedgerEntry> ApplyDefaultSorting(IQueryable<ProjectLedgerEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

[Authorize(ErpPermissions.Projects.Default)]
public class ResourceLedgerEntryAppService : ErpReadOnlyAppService<ResourceLedgerEntry, ResourceLedgerEntryDto, Guid, GetProjectLedgerListInput>, IResourceLedgerEntryAppService
{
    public ResourceLedgerEntryAppService(IRepository<ResourceLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Projects.Default;
        GetListPolicyName = ErpPermissions.Projects.Default;
    }

    protected override async Task<IQueryable<ResourceLedgerEntry>> CreateFilteredQueryAsync(GetProjectLedgerListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ResourceNo.IsNullOrWhiteSpace(), e => e.ResourceNo == input.ResourceNo)
            .WhereIf(!input.ProjectNo.IsNullOrWhiteSpace(), e => e.ProjectNo == input.ProjectNo)
            .WhereIf(!filter.IsNullOrEmpty(), e => e.ResourceNo.ToLower().Contains(filter) || (e.DocumentNo != null && e.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<ResourceLedgerEntry> ApplyDefaultSorting(IQueryable<ResourceLedgerEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

/// <summary>Time sheets: a week of hours per project task, submitted, approved and moved to the project journal.</summary>
[Authorize(ErpPermissions.TimeSheets.Default)]
public class TimeSheetAppService
    : ErpCrudAppService<TimeSheetHeader, TimeSheetDto, Guid, GetTimeSheetListInput, CreateUpdateTimeSheetDto, CreateUpdateTimeSheetDto>,
        ITimeSheetAppService
{
    public TimeSheetAppService(IRepository<TimeSheetHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.TimeSheets.Default;
        GetListPolicyName = ErpPermissions.TimeSheets.Default;
        CreatePolicyName = ErpPermissions.TimeSheets.Create;
        UpdatePolicyName = ErpPermissions.TimeSheets.Update;
        DeletePolicyName = ErpPermissions.TimeSheets.Delete;
    }

    protected override async Task<TimeSheetHeader> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<TimeSheetDto> CreateAsync(CreateUpdateTimeSheetDto input)
    {
        await CheckCreatePolicyAsync();
        var resource = await LazyServiceProvider.LazyGetRequiredService<ProjectManager>().GetResourceAsync(input.ResourceNo);
        var setup = await ProjectNumbering.SetupAsync(LazyServiceProvider);
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup?.TimeSheetNos, input.No, Clock.Now);
        var sheet = new TimeSheetHeader(GuidGenerator.Create(), no, resource, input.StartingDate == default ? Clock.Now.Date : input.StartingDate);
        await UpsertLinesAsync(sheet, input);
        await Repository.InsertAsync(sheet, autoSave: true);
        return await MapToGetOutputDtoAsync(sheet);
    }

    public override async Task<TimeSheetDto> UpdateAsync(Guid id, CreateUpdateTimeSheetDto input)
    {
        await CheckUpdatePolicyAsync();
        var sheet = await GetEntityByIdAsync(id);
        sheet.EnsureOpen();
        await UpsertLinesAsync(sheet, input);
        await Repository.UpdateAsync(sheet, autoSave: true);
        return await MapToGetOutputDtoAsync(sheet);
    }

    public async Task<TimeSheetDto> SubmitAsync(Guid id) => await ChangeAsync(id, s => s.Submit(), ErpPermissions.TimeSheets.Update);

    public async Task<TimeSheetDto> ApproveAsync(Guid id) => await ChangeAsync(id, s => s.Approve(), ErpPermissions.TimeSheets.Approve);

    public async Task<TimeSheetDto> RejectAsync(Guid id, RejectTimeSheetInput input) => await ChangeAsync(id, s => s.Reject(input.Comment), ErpPermissions.TimeSheets.Approve);

    public async Task<TimeSheetDto> ReopenAsync(Guid id) => await ChangeAsync(id, s => s.Reopen(), ErpPermissions.TimeSheets.Approve);

    [Authorize(ErpPermissions.Projects.Post)]
    public Task<int> TransferToJournalAsync(Guid id, TransferTimeSheetInput input) =>
        LazyServiceProvider.LazyGetRequiredService<ProjectJnlPostBatch>().TransferTimeSheetAsync(id, input.ProjectJournalBatchId);

    private async Task<TimeSheetDto> ChangeAsync(Guid id, Action<TimeSheetHeader> change, string policy)
    {
        await AuthorizationService.CheckAsync(policy);
        var sheet = await GetEntityByIdAsync(id);
        change(sheet);
        await Repository.UpdateAsync(sheet, autoSave: true);
        return await MapToGetOutputDtoAsync(sheet);
    }

    private async Task UpsertLinesAsync(TimeSheetHeader sheet, CreateUpdateTimeSheetDto input)
    {
        var manager = LazyServiceProvider.LazyGetRequiredService<ProjectManager>();
        var projects = LazyServiceProvider.LazyGetRequiredService<IRepository<Project, Guid>>();
        var sent = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
        sheet.Lines.RemoveAll(l => !sent.Contains(l.Id) && !l.Posted);

        foreach (var lineInput in input.Lines)
        {
            var project = await projects.GetAsync((await manager.GetProjectAsync(lineInput.ProjectNo)).Id, includeDetails: true);
            project.GetPostingTask(lineInput.TaskNo);

            var line = lineInput.Id.HasValue ? sheet.Lines.FirstOrDefault(l => l.Id == lineInput.Id.Value) : null;
            if (line == null)
            {
                line = new TimeSheetLine(GuidGenerator.Create(), sheet.Id, (sheet.Lines.Count == 0 ? 0 : sheet.Lines.Max(l => l.LineNo)) + 10000);
                sheet.Lines.Add(line);
            }

            var days = Enumerable.Range(0, 7).Select(i => i < lineInput.Days.Count ? lineInput.Days[i] : 0m).ToArray();
            line.Set(project.No, lineInput.TaskNo, lineInput.Description, lineInput.Chargeable, days);
        }
    }

    protected override Task<TimeSheetDto> MapToGetOutputDtoAsync(TimeSheetHeader entity) => Task.FromResult(Map(entity));

    protected override Task<List<TimeSheetDto>> MapToGetListOutputDtosAsync(List<TimeSheetHeader> entities) => Task.FromResult(entities.Select(Map).ToList());

    private static TimeSheetDto Map(TimeSheetHeader s) => new()
    {
        Id = s.Id,
        No = s.No,
        ResourceNo = s.ResourceNo,
        ResourceName = s.ResourceName,
        StartingDate = s.StartingDate,
        EndingDate = s.EndingDate,
        Status = s.Status,
        ApproverComment = s.ApproverComment,
        TotalHours = s.TotalHours,
        Lines = s.Lines.OrderBy(l => l.LineNo).Select(l => new TimeSheetLineDto
        {
            Id = l.Id, LineNo = l.LineNo, ProjectNo = l.ProjectNo, TaskNo = l.TaskNo, Description = l.Description, Chargeable = l.Chargeable,
            Days = l.Days.ToList(), TotalHours = l.TotalHours, Posted = l.Posted,
        }).ToList(),
    };

    protected override async Task<IQueryable<TimeSheetHeader>> CreateFilteredQueryAsync(GetTimeSheetListInput input)
    {
        var query = await Repository.WithDetailsAsync();
        return query
            .WhereIf(!input.ResourceNo.IsNullOrWhiteSpace(), s => s.ResourceNo == input.ResourceNo)
            .WhereIf(input.Status.HasValue, s => s.Status == input.Status.Value);
    }

    protected override IQueryable<TimeSheetHeader> ApplyDefaultSorting(IQueryable<TimeSheetHeader> query) => query.OrderByDescending(s => s.StartingDate).ThenBy(s => s.ResourceNo);
}
