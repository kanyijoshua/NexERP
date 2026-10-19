using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Projects;

/// <summary>Projects Setup: number series and the posting group a new project takes.</summary>
public class ProjectsSetup : CompanyEntity
{
    public string ProjectNos { get; private set; }
    public string ResourceNos { get; private set; }
    public string TimeSheetNos { get; private set; }
    public string DefaultPostingGroup { get; private set; }

    protected ProjectsSetup() { }

    public ProjectsSetup(Guid id)
        : base(id) { }

    public void Set(string projectNos, string resourceNos, string timeSheetNos, string defaultPostingGroup)
    {
        ProjectNos = Code(projectNos, nameof(projectNos));
        ResourceNos = Code(resourceNos, nameof(resourceNos));
        TimeSheetNos = Code(timeSheetNos, nameof(timeSheetNos));
        DefaultPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(defaultPostingGroup, nameof(defaultPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
    }

    private static string Code(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
}

/// <summary>Project Posting Group: where the costs and sales of a project and its work in progress post.</summary>
public class ProjectPostingGroup : CodeTableEntity
{
    /// <summary>Debited with item and G/L usage.</summary>
    public string ProjectCostsAccount { get; private set; }

    /// <summary>Credited with what a project invoice bills.</summary>
    public string ProjectSalesAccount { get; private set; }

    public string WipCostsAccount { get; private set; }
    public string ProjectCostsAppliedAccount { get; private set; }
    public string WipSalesAccount { get; private set; }
    public string RecognizedSalesAccount { get; private set; }

    protected ProjectPostingGroup() { }

    public ProjectPostingGroup(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetAccounts(string projectCostsAccount, string projectSalesAccount, string wipCostsAccount, string projectCostsAppliedAccount, string wipSalesAccount, string recognizedSalesAccount)
    {
        ProjectCostsAccount = Account(projectCostsAccount, nameof(projectCostsAccount));
        ProjectSalesAccount = Account(projectSalesAccount, nameof(projectSalesAccount));
        WipCostsAccount = Account(wipCostsAccount, nameof(wipCostsAccount));
        ProjectCostsAppliedAccount = Account(projectCostsAppliedAccount, nameof(projectCostsAppliedAccount));
        WipSalesAccount = Account(wipSalesAccount, nameof(wipSalesAccount));
        RecognizedSalesAccount = Account(recognizedSalesAccount, nameof(recognizedSalesAccount));
    }

    private static string Account(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>Resource: a person or a machine whose time is planned, used and billed on projects.</summary>
public class Resource : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Name { get; private set; }
    public ResourceType Type { get; private set; }
    public string EmployeeNo { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>Hours a day the resource is available.</summary>
    public decimal CapacityPerDay { get; private set; } = 8m;

    public bool Blocked { get; private set; }

    protected Resource() { }

    public Resource(Guid id, string no, string name, ResourceType type)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetName(name);
        Type = type;
    }

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();

    public void Set(ResourceType type, string employeeNo, string unitOfMeasureCode, decimal unitCost, decimal unitPrice, decimal capacityPerDay, bool blocked)
    {
        Type = type;
        EmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength));
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
        UnitCost = Math.Max(0m, unitCost);
        UnitPrice = Math.Max(0m, unitPrice);
        CapacityPerDay = Math.Max(0m, capacityPerDay);
        Blocked = blocked;
    }
}

/// <summary>Project: work done for a customer, planned by task, its costs and sales posted to the project ledger.</summary>
public class Project : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public ProjectStatus Status { get; private set; }
    public string PostingGroup { get; private set; }
    public ProjectWipMethod WipMethod { get; private set; }
    public DateTime? StartingDate { get; private set; }
    public DateTime? EndingDate { get; private set; }
    public string PersonResponsible { get; private set; }

    public List<ProjectTask> Tasks { get; private set; } = new();

    protected Project() { }

    public Project(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetDescription(description);
    }

    public void SetDescription(string description) => Description = Check.NotNullOrWhiteSpace(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    public void SetCustomer(Guid? customerId, string customerNo, string customerName)
    {
        CustomerId = customerId;
        CustomerNo = customerNo;
        CustomerName = customerName;
    }

    public void Set(ProjectStatus status, string postingGroup, ProjectWipMethod wipMethod, DateTime? startingDate, DateTime? endingDate, string personResponsible)
    {
        Status = status;
        PostingGroup = CodeTableEntity.NormalizeCode(Check.Length(postingGroup, nameof(postingGroup), ErpDomainConsts.MaxPostingGroupLength));
        WipMethod = wipMethod;
        StartingDate = startingDate?.Date;
        EndingDate = endingDate?.Date;
        PersonResponsible = CodeTableEntity.NormalizeCode(Check.Length(personResponsible, nameof(personResponsible), ErpDomainConsts.MaxCodeLength));
    }

    /// <summary>A completed project takes no more usage.</summary>
    public void EnsurePostable()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Planning)
        {
            throw new BusinessException(ErpErrorCodes.Projects.ProjectNotOpen).WithData("no", No).WithData("status", Status.ToString());
        }
    }

    public ProjectTask FindTask(string taskNo) => Tasks.FirstOrDefault(t => t.TaskNo == taskNo?.Trim().ToUpperInvariant());

    public ProjectTask GetPostingTask(string taskNo)
    {
        var task = FindTask(taskNo) ?? throw new BusinessException(ErpErrorCodes.Projects.TaskNotFound).WithData("no", No).WithData("task", taskNo ?? "");
        if (task.TaskType != ProjectTaskType.Posting)
        {
            throw new BusinessException(ErpErrorCodes.Projects.TaskNotPosting).WithData("no", No).WithData("task", task.TaskNo);
        }

        return task;
    }
}

/// <summary>A task of a project: posting tasks take entries; headings and totals structure the tree.</summary>
public class ProjectTask : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string TaskNo { get; private set; }
    public string Description { get; private set; }
    public ProjectTaskType TaskType { get; private set; }
    public int Indentation { get; private set; }

    protected ProjectTask() { }

    public ProjectTask(Guid id, Guid projectId, string taskNo, string description, ProjectTaskType taskType, int indentation)
        : base(id)
    {
        ProjectId = projectId;
        Update(taskNo, description, taskType, indentation);
    }

    public void Update(string taskNo, string description, ProjectTaskType taskType, int indentation)
    {
        TaskNo = Check.NotNullOrWhiteSpace(taskNo, nameof(taskNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        TaskType = taskType;
        Indentation = Math.Max(0, indentation);
    }
}

/// <summary>A planned use of a resource, item or G/L account on a task: a budget, something to bill, or both.</summary>
public class ProjectPlanningLine : CompanyEntity
{
    public string ProjectNo { get; private set; }
    public string TaskNo { get; private set; }
    public int LineNo { get; private set; }
    public ProjectPlanningLineType LineType { get; private set; }
    public ProjectLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public DateTime PlanningDate { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>What has been put on a sales invoice so far.</summary>
    public decimal QtyTransferredToInvoice { get; private set; }

    public string InvoiceNo { get; private set; }

    protected ProjectPlanningLine() { }

    public ProjectPlanningLine(Guid id, string projectNo, string taskNo, int lineNo)
        : base(id)
    {
        ProjectNo = projectNo;
        TaskNo = taskNo;
        LineNo = lineNo;
    }

    public decimal TotalCost => Math.Round(Quantity * UnitCost, 2);
    public decimal TotalPrice => Math.Round(Quantity * UnitPrice, 2);
    public bool IsBudget => LineType != ProjectPlanningLineType.Billable;
    public bool IsBillable => LineType != ProjectPlanningLineType.Budget;
    public decimal QtyToInvoice => IsBillable ? Quantity - QtyTransferredToInvoice : 0m;

    public void Set(string taskNo, ProjectPlanningLineType lineType, ProjectLineType type, string no, string description, DateTime planningDate, decimal quantity, decimal unitCost, decimal unitPrice)
    {
        TaskNo = Check.NotNullOrWhiteSpace(taskNo, nameof(taskNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        LineType = lineType;
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        PlanningDate = planningDate.Date;
        Quantity = quantity;
        UnitCost = unitCost;
        UnitPrice = unitPrice;
    }

    public void TransferToInvoice(decimal quantity, string invoiceNo)
    {
        QtyTransferredToInvoice += quantity;
        InvoiceNo = invoiceNo;
    }
}

/// <summary>A project journal batch: lines of usage waiting to be posted.</summary>
public class ProjectJournalBatch : CompanyAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    protected ProjectJournalBatch() { }

    public ProjectJournalBatch(Guid id, string name, string description)
        : base(id)
    {
        SetName(name);
        SetDescription(description);
    }

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxJournalTemplateNameLength).Trim().ToUpperInvariant();

    public void SetDescription(string description) => Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
}

/// <summary>Usage of a resource, an item or a G/L account on a project task.</summary>
public class ProjectJournalLine : CompanyEntity
{
    public Guid ProjectJournalBatchId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public string ProjectNo { get; private set; }
    public string TaskNo { get; private set; }
    public ProjectLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }

    /// <summary>Blank takes the resource's or item's own cost and price.</summary>
    public decimal? UnitCost { get; private set; }

    public decimal? UnitPrice { get; private set; }
    public bool Chargeable { get; private set; } = true;
    public string LocationCode { get; private set; }

    /// <summary>The account a G/L line is paid from.</summary>
    public string BalAccountNo { get; private set; }

    /// <summary>The time sheet line the usage was taken from.</summary>
    public Guid? TimeSheetLineId { get; private set; }

    protected ProjectJournalLine() { }

    public ProjectJournalLine(Guid id, Guid batchId, int lineNo)
        : base(id)
    {
        ProjectJournalBatchId = batchId;
        LineNo = lineNo;
    }

    public void Set(DateTime postingDate, string documentNo, string projectNo, string taskNo, ProjectLineType type, string no, string description, decimal quantity, decimal? unitCost, decimal? unitPrice, bool chargeable)
    {
        PostingDate = postingDate.Date;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength).Trim();
        ProjectNo = Check.NotNullOrWhiteSpace(projectNo, nameof(projectNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        TaskNo = Check.NotNullOrWhiteSpace(taskNo, nameof(taskNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        UnitCost = unitCost;
        UnitPrice = unitPrice;
        Chargeable = chargeable;
    }

    public void SetPosting(string locationCode, string balAccountNo)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        BalAccountNo = balAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(balAccountNo.Trim(), nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
    }

    internal void LinkTimeSheet(Guid timeSheetLineId) => TimeSheetLineId = timeSheetLineId;
}

/// <summary>Project Ledger Entry: a posted usage or sale on a project task.</summary>
public class ProjectLedgerEntry : LedgerEntryBase
{
    public string ProjectNo { get; internal set; }
    public string TaskNo { get; internal set; }
    public DateTime PostingDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public ProjectLedgerEntryType EntryType { get; internal set; }
    public ProjectLineType Type { get; internal set; }
    public string No { get; internal set; }
    public string Description { get; internal set; }
    public decimal Quantity { get; internal set; }
    public decimal UnitCost { get; internal set; }
    public decimal TotalCost { get; internal set; }
    public decimal UnitPrice { get; internal set; }
    public decimal TotalPrice { get; internal set; }
    public bool Chargeable { get; internal set; }
    public long TransactionNo { get; internal set; }

    protected ProjectLedgerEntry() { }

    public ProjectLedgerEntry(Guid id)
        : base(id) { }
}

/// <summary>Resource Ledger Entry: the time a resource spent, at its cost and price.</summary>
public class ResourceLedgerEntry : LedgerEntryBase
{
    public string ResourceNo { get; internal set; }
    public DateTime PostingDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public string ProjectNo { get; internal set; }
    public string TaskNo { get; internal set; }
    public string Description { get; internal set; }
    public decimal Quantity { get; internal set; }
    public decimal UnitCost { get; internal set; }
    public decimal TotalCost { get; internal set; }
    public decimal UnitPrice { get; internal set; }
    public decimal TotalPrice { get; internal set; }
    public bool Chargeable { get; internal set; }

    protected ResourceLedgerEntry() { }

    public ResourceLedgerEntry(Guid id)
        : base(id) { }
}

/// <summary>A WIP posting of a project: what the last WIP run moved, reversed by the next one.</summary>
public class ProjectWipEntry : CompanyEntity
{
    public string ProjectNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public ProjectWipEntryType Type { get; private set; }
    public string AccountNo { get; private set; }
    public string BalAccountNo { get; private set; }
    public decimal Amount { get; private set; }
    public bool Reversed { get; private set; }

    protected ProjectWipEntry() { }

    internal ProjectWipEntry(Guid id, string projectNo, DateTime postingDate, ProjectWipEntryType type, string accountNo, string balAccountNo, decimal amount)
        : base(id)
    {
        ProjectNo = projectNo;
        PostingDate = postingDate.Date;
        Type = type;
        AccountNo = accountNo;
        BalAccountNo = balAccountNo;
        Amount = amount;
    }

    internal void MarkReversed() => Reversed = true;
}

/// <summary>A week of a resource's time, by project task and day, submitted and approved before it is posted.</summary>
public class TimeSheetHeader : CompanyAggregateRoot
{
    public string No { get; private set; }
    public string ResourceNo { get; private set; }
    public string ResourceName { get; private set; }

    /// <summary>The Monday the week starts on.</summary>
    public DateTime StartingDate { get; private set; }

    public TimeSheetStatus Status { get; private set; }
    public string ApproverComment { get; private set; }

    public List<TimeSheetLine> Lines { get; private set; } = new();

    protected TimeSheetHeader() { }

    public TimeSheetHeader(Guid id, string no, Resource resource, DateTime startingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        ResourceNo = resource.No;
        ResourceName = resource.Name;
        var date = startingDate.Date;
        StartingDate = date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }

    public DateTime EndingDate => StartingDate.AddDays(6);

    public decimal TotalHours => Lines.Sum(l => l.TotalHours);

    public void EnsureOpen()
    {
        if (Status is TimeSheetStatus.Submitted or TimeSheetStatus.Approved)
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetNotOpen).WithData("no", No).WithData("status", Status.ToString());
        }
    }

    internal void SetStatus(TimeSheetStatus status, string comment = null)
    {
        Status = status;
        ApproverComment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Hands the week in for approval; it can no longer be changed.</summary>
    public void Submit()
    {
        EnsureOpen();
        if (!Lines.Any(l => l.TotalHours > 0m))
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetEmpty).WithData("no", No);
        }

        SetStatus(TimeSheetStatus.Submitted);
    }

    public void Approve()
    {
        if (Status != TimeSheetStatus.Submitted)
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetNotSubmitted).WithData("no", No);
        }

        SetStatus(TimeSheetStatus.Approved);
    }

    public void Reject(string comment)
    {
        if (Status != TimeSheetStatus.Submitted)
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetNotSubmitted).WithData("no", No);
        }

        SetStatus(TimeSheetStatus.Rejected, comment);
    }

    /// <summary>Opens a submitted or rejected week for changes again; an approved one only while nothing of it is posted.</summary>
    public void Reopen()
    {
        if (Lines.Any(l => l.Posted))
        {
            throw new BusinessException(ErpErrorCodes.Projects.TimeSheetPosted).WithData("no", No);
        }

        SetStatus(TimeSheetStatus.Open);
    }
}

/// <summary>The hours of one project task in a time sheet week, Monday to Sunday.</summary>
public class TimeSheetLine : Entity<Guid>
{
    public Guid TimeSheetHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public string ProjectNo { get; private set; }
    public string TaskNo { get; private set; }
    public string Description { get; private set; }
    public bool Chargeable { get; private set; } = true;
    public decimal Day1 { get; private set; }
    public decimal Day2 { get; private set; }
    public decimal Day3 { get; private set; }
    public decimal Day4 { get; private set; }
    public decimal Day5 { get; private set; }
    public decimal Day6 { get; private set; }
    public decimal Day7 { get; private set; }

    /// <summary>Set once the line has been moved to the project journal.</summary>
    public bool Posted { get; private set; }

    protected TimeSheetLine() { }

    public TimeSheetLine(Guid id, Guid headerId, int lineNo)
        : base(id)
    {
        TimeSheetHeaderId = headerId;
        LineNo = lineNo;
    }

    public decimal TotalHours => Day1 + Day2 + Day3 + Day4 + Day5 + Day6 + Day7;

    public decimal[] Days => [Day1, Day2, Day3, Day4, Day5, Day6, Day7];

    public void Set(string projectNo, string taskNo, string description, bool chargeable, decimal[] days)
    {
        if (days == null || days.Length != 7 || days.Any(d => d < 0m || d > 24m))
        {
            throw new BusinessException(ErpErrorCodes.Projects.InvalidHours);
        }

        ProjectNo = Check.NotNullOrWhiteSpace(projectNo, nameof(projectNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        TaskNo = Check.NotNullOrWhiteSpace(taskNo, nameof(taskNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Chargeable = chargeable;
        (Day1, Day2, Day3, Day4, Day5, Day6, Day7) = (days[0], days[1], days[2], days[3], days[4], days[5], days[6]);
    }

    internal void MarkPosted() => Posted = true;
}
