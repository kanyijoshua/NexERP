using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.JobQueue;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.MaintenanceManagement;

/// <summary>Maintenance Team: the people a request is assigned to.</summary>
public class MaintenanceTeam : CodeTableEntity
{
    protected MaintenanceTeam() { }

    public MaintenanceTeam(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>
/// Equipment: a machine or tool that is maintained, often a fixed asset. Preventive maintenance
/// comes round every interval from the last time it was maintained.
/// </summary>
public class MaintenanceEquipment : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public string SerialNo { get; private set; }
    public string FixedAssetNo { get; private set; }
    public string LocationCode { get; private set; }
    public string TeamCode { get; private set; }
    public string TechnicianEmployeeNo { get; private set; }

    /// <summary>Days between preventive maintenance; 0 is none.</summary>
    public int PreventiveIntervalDays { get; private set; }

    public DateTime? NextPreventiveDate { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }
    public EquipmentStatus Status { get; private set; }

    protected MaintenanceEquipment() { }

    public MaintenanceEquipment(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, string category, string serialNo, string fixedAssetNo, string locationCode, string teamCode, string technicianEmployeeNo)
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), ErpDomainConsts.MaxDescriptionLength).Trim();
        Category = Check.Length(category, nameof(category), ErpDomainConsts.MaxNameLength);
        SerialNo = Check.Length(serialNo?.Trim(), nameof(serialNo), ErpDomainConsts.MaxNoLength);
        FixedAssetNo = CodeTableEntity.NormalizeCode(Check.Length(fixedAssetNo, nameof(fixedAssetNo), ErpDomainConsts.MaxNoLength));
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        TeamCode = CodeTableEntity.NormalizeCode(Check.Length(teamCode, nameof(teamCode), ErpDomainConsts.MaxCodeLength));
        TechnicianEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(technicianEmployeeNo, nameof(technicianEmployeeNo), ErpDomainConsts.MaxNoLength));
    }

    /// <summary>A new interval starts counting from the next date given, else from today.</summary>
    public void SetPreventive(int intervalDays, DateTime? nextDate, DateTime today)
    {
        PreventiveIntervalDays = Math.Max(0, intervalDays);
        NextPreventiveDate = PreventiveIntervalDays == 0 ? null : (nextDate ?? NextPreventiveDate ?? today.AddDays(PreventiveIntervalDays)).Date;
    }

    public void EnsureActive()
    {
        if (Status == EquipmentStatus.Scrapped)
        {
            throw new BusinessException(ErpErrorCodes.Maintenance.EquipmentScrapped).WithData("no", No);
        }
    }

    internal void RecordMaintenance(DateTime date)
    {
        LastMaintenanceDate = date.Date;
        if (PreventiveIntervalDays > 0)
        {
            NextPreventiveDate = date.Date.AddDays(PreventiveIntervalDays);
        }
    }

    internal void Scrap()
    {
        Status = EquipmentStatus.Scrapped;
        NextPreventiveDate = null;
    }
}

/// <summary>Maintenance Request: a repair or a preventive job on one piece of equipment, from new to repaired (or scrapped).</summary>
public class MaintenanceRequest : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string EquipmentNo { get; private set; }
    public string Subject { get; private set; }
    public MaintenanceType Type { get; private set; }
    public MaintenancePriority Priority { get; private set; } = MaintenancePriority.Normal;
    public MaintenanceStage Stage { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string TeamCode { get; private set; }
    public string TechnicianEmployeeNo { get; private set; }
    public decimal DurationHours { get; private set; }
    public decimal Cost { get; private set; }
    public string Notes { get; private set; }

    protected MaintenanceRequest() { }

    public MaintenanceRequest(Guid id, string no, MaintenanceEquipment equipment, MaintenanceType type, DateTime requestDate)
        : base(id)
    {
        No = no;
        EquipmentNo = equipment.No;
        Type = type;
        RequestDate = requestDate.Date;
        TeamCode = equipment.TeamCode;
        TechnicianEmployeeNo = equipment.TechnicianEmployeeNo;
        Stage = MaintenanceStage.New;
    }

    public void Set(string subject, MaintenancePriority priority, DateTime? scheduledDate, string teamCode, string technicianEmployeeNo, decimal durationHours, decimal cost, string notes)
    {
        EnsureOpen();
        Subject = Check.NotNullOrWhiteSpace(subject, nameof(subject), ErpDomainConsts.MaxDescriptionLength).Trim();
        Priority = priority;
        ScheduledDate = scheduledDate?.Date;
        TeamCode = CodeTableEntity.NormalizeCode(Check.Length(teamCode, nameof(teamCode), ErpDomainConsts.MaxCodeLength));
        TechnicianEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(technicianEmployeeNo, nameof(technicianEmployeeNo), ErpDomainConsts.MaxNoLength));
        DurationHours = Math.Max(0m, durationHours);
        Cost = Math.Max(0m, cost);
        Notes = Check.Length(notes, nameof(notes), 2000);
    }

    public void Start()
    {
        EnsureOpen();
        Stage = MaintenanceStage.InProgress;
    }

    internal void Finish(MaintenanceStage stage, DateTime date)
    {
        EnsureOpen();
        Stage = stage;
        CompletedDate = date.Date;
    }

    public bool IsOpen => Stage is MaintenanceStage.New or MaintenanceStage.InProgress;

    private void EnsureOpen()
    {
        if (!IsOpen)
        {
            throw new BusinessException(ErpErrorCodes.Maintenance.StageInvalid).WithData("no", No).WithData("stage", Stage.ToString());
        }
    }
}

/// <summary>Raises preventive requests when they fall due and closes requests, keeping the equipment's dates.</summary>
public class MaintenanceManager : DomainService
{
    private readonly IRepository<MaintenanceEquipment, Guid> _equipment;
    private readonly IRepository<MaintenanceRequest, Guid> _requests;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public MaintenanceManager(IRepository<MaintenanceEquipment, Guid> equipment, IRepository<MaintenanceRequest, Guid> requests)
    {
        _equipment = equipment;
        _requests = requests;
    }

    public async Task<string> NextRequestNoAsync() => $"MR{await EntryNos.NextAsync(ErpSequenceNames.MaintenanceRequestNo):D5}";

    public async Task<string> NextEquipmentNoAsync() => $"EQ{await EntryNos.NextAsync(ErpSequenceNames.EquipmentNo):D5}";

    public async Task<MaintenanceEquipment> GetEquipmentAsync(string no)
    {
        var normalized = no?.Trim().ToUpperInvariant();
        return await _equipment.FirstOrDefaultAsync(e => e.No == normalized)
            ?? throw new BusinessException(ErpErrorCodes.Maintenance.EquipmentNotFound).WithData("no", normalized ?? "");
    }

    /// <summary>
    /// A preventive request for every active piece of equipment due by the date that has no open
    /// preventive request already; returns the requests made.
    /// </summary>
    public async Task<int> GeneratePreventiveRequestsAsync(DateTime asOf)
    {
        var date = asOf.Date;
        var due = await _equipment.GetListAsync(e => e.Status == EquipmentStatus.Active && e.NextPreventiveDate != null && e.NextPreventiveDate <= date);
        var made = 0;
        foreach (var equipment in due.OrderBy(e => e.No))
        {
            var equipmentNo = equipment.No;
            var open = await _requests.AnyAsync(r => r.EquipmentNo == equipmentNo && r.Type == MaintenanceType.Preventive
                && (r.Stage == MaintenanceStage.New || r.Stage == MaintenanceStage.InProgress));
            if (open)
            {
                continue;
            }

            var request = new MaintenanceRequest(GuidGenerator.Create(), await NextRequestNoAsync(), equipment, MaintenanceType.Preventive, date);
            request.Set($"Preventive maintenance {equipment.Description}", MaintenancePriority.Normal, equipment.NextPreventiveDate, equipment.TeamCode, equipment.TechnicianEmployeeNo, 0m, 0m, null);
            await _requests.InsertAsync(request, autoSave: true);
            made++;
        }

        return made;
    }

    /// <summary>The work is done: the equipment was maintained today and its next preventive date moves on.</summary>
    public async Task<MaintenanceRequest> CompleteAsync(Guid requestId, DateTime date)
    {
        var request = await _requests.GetAsync(requestId);
        request.Finish(MaintenanceStage.Repaired, date);
        var equipment = await GetEquipmentAsync(request.EquipmentNo);
        equipment.RecordMaintenance(date);
        await _equipment.UpdateAsync(equipment, autoSave: true);
        return await _requests.UpdateAsync(request, autoSave: true);
    }

    /// <summary>The equipment cannot be repaired: the request ends and the equipment is scrapped.</summary>
    public async Task<MaintenanceRequest> ScrapAsync(Guid requestId, DateTime date)
    {
        var request = await _requests.GetAsync(requestId);
        request.Finish(MaintenanceStage.Scrapped, date);
        var equipment = await GetEquipmentAsync(request.EquipmentNo);
        equipment.Scrap();
        await _equipment.UpdateAsync(equipment, autoSave: true);
        return await _requests.UpdateAsync(request, autoSave: true);
    }
}

/// <summary>Raises the preventive maintenance requests that have fallen due: run it daily.</summary>
public class PreventiveMaintenanceJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "GeneratePreventiveMaintenance";
    public string DisplayName => "Generate Preventive Maintenance";
    public string Description => "Creates a preventive maintenance request for every piece of equipment whose next preventive date has come.";
    public string DefaultParametersJson => "{}";

    private readonly MaintenanceManager _manager;

    public PreventiveMaintenanceJobHandler(MaintenanceManager manager)
    {
        _manager = manager;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var made = await _manager.GeneratePreventiveRequestsAsync(DateTime.Today);
        return JobExecutionResult.Ok(made, $"Created {made} preventive maintenance request(s).");
    }
}
