using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.MaintenanceManagement;

public class MaintenanceTeamAppService : CodeTableAppServiceBase<MaintenanceTeam, MaintenanceTeamDto, CreateUpdateMaintenanceTeamDto>, IMaintenanceTeamAppService
{
    public MaintenanceTeamAppService(IRepository<MaintenanceTeam, Guid> repository)
        : base(repository, ErpPermissions.Maintenance.Default) { }

    protected override MaintenanceTeam NewEntity(Guid id, CreateUpdateMaintenanceTeamDto input) => new(id, input.Code, input.Description);
}

[Authorize(ErpPermissions.Maintenance.Default)]
public class MaintenanceEquipmentAppService
    : ErpTableAppService<MaintenanceEquipment, MaintenanceEquipmentDto, GetMaintenanceListInput, CreateUpdateMaintenanceEquipmentDto>,
        IMaintenanceEquipmentAppService
{
    private MaintenanceManager Manager => LazyServiceProvider.LazyGetRequiredService<MaintenanceManager>();

    public MaintenanceEquipmentAppService(IRepository<MaintenanceEquipment, Guid> repository)
        : base(repository, ErpPermissions.Maintenance.Default) { }

    public override async Task<MaintenanceEquipmentDto> CreateAsync(CreateUpdateMaintenanceEquipmentDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextEquipmentNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(e => e.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var equipment = new MaintenanceEquipment(GuidGenerator.Create(), no);
        await ApplyAsync(equipment, input);
        await Repository.InsertAsync(equipment, autoSave: true);
        return await MapToGetOutputDtoAsync(equipment);
    }

    public override async Task<MaintenanceEquipmentDto> UpdateAsync(Guid id, CreateUpdateMaintenanceEquipmentDto input)
    {
        await CheckUpdatePolicyAsync();
        var equipment = await GetEntityByIdAsync(id);
        await ApplyAsync(equipment, input);
        await Repository.UpdateAsync(equipment, autoSave: true);
        return await MapToGetOutputDtoAsync(equipment);
    }

    private async Task ApplyAsync(MaintenanceEquipment equipment, CreateUpdateMaintenanceEquipmentDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<MaintenanceTeam>(input.TeamCode);
        await CodeTableChecker.EnsureExistsAsync<Location>(input.LocationCode);
        equipment.Set(input.Description, input.Category, input.SerialNo, input.FixedAssetNo, input.LocationCode, input.TeamCode, input.TechnicianEmployeeNo);
        equipment.SetPreventive(input.PreventiveIntervalDays, input.NextPreventiveDate, Clock.Now);
    }

    protected override async Task<IQueryable<MaintenanceEquipment>> CreateFilteredQueryAsync(GetMaintenanceListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), e => e.No.ToLower().Contains(filter) || e.Description.ToLower().Contains(filter) || (e.SerialNo != null && e.SerialNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<MaintenanceEquipment> ApplyDefaultSorting(IQueryable<MaintenanceEquipment> query) => query.OrderBy(e => e.No);
}

[Authorize(ErpPermissions.Maintenance.Default)]
public class MaintenanceRequestAppService
    : ErpTableAppService<MaintenanceRequest, MaintenanceRequestDto, GetMaintenanceListInput, CreateUpdateMaintenanceRequestDto>,
        IMaintenanceRequestAppService
{
    private MaintenanceManager Manager => LazyServiceProvider.LazyGetRequiredService<MaintenanceManager>();

    public MaintenanceRequestAppService(IRepository<MaintenanceRequest, Guid> repository)
        : base(repository, ErpPermissions.Maintenance.Default) { }

    public override async Task<MaintenanceRequestDto> CreateAsync(CreateUpdateMaintenanceRequestDto input)
    {
        await CheckCreatePolicyAsync();
        var equipment = await Manager.GetEquipmentAsync(input.EquipmentNo);
        equipment.EnsureActive();
        var request = new MaintenanceRequest(GuidGenerator.Create(), await Manager.NextRequestNoAsync(), equipment, input.Type, input.RequestDate ?? Clock.Now.Date);
        await ApplyAsync(request, input);
        await Repository.InsertAsync(request, autoSave: true);
        return await MapToGetOutputDtoAsync(request);
    }

    public override async Task<MaintenanceRequestDto> UpdateAsync(Guid id, CreateUpdateMaintenanceRequestDto input)
    {
        await CheckUpdatePolicyAsync();
        var request = await GetEntityByIdAsync(id);
        await ApplyAsync(request, input);
        await Repository.UpdateAsync(request, autoSave: true);
        return await MapToGetOutputDtoAsync(request);
    }

    [Authorize(ErpPermissions.Maintenance.Update)]
    public async Task<MaintenanceRequestDto> StartAsync(Guid id)
    {
        var request = await GetEntityByIdAsync(id);
        request.Start();
        await Repository.UpdateAsync(request, autoSave: true);
        return await MapToGetOutputDtoAsync(request);
    }

    [Authorize(ErpPermissions.Maintenance.Update)]
    public async Task<MaintenanceRequestDto> CompleteAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.CompleteAsync(id, Clock.Now));

    [Authorize(ErpPermissions.Maintenance.Update)]
    public async Task<MaintenanceRequestDto> ScrapAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.ScrapAsync(id, Clock.Now));

    [Authorize(ErpPermissions.Maintenance.Create)]
    public async Task<GeneratePreventiveResultDto> GeneratePreventiveAsync(GeneratePreventiveInput input) =>
        new() { RequestsCreated = await Manager.GeneratePreventiveRequestsAsync(input?.AsOfDate ?? Clock.Now.Date) };

    private async Task ApplyAsync(MaintenanceRequest request, CreateUpdateMaintenanceRequestDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<MaintenanceTeam>(input.TeamCode);
        request.Set(input.Subject, input.Priority, input.ScheduledDate, input.TeamCode, input.TechnicianEmployeeNo, input.DurationHours, input.Cost, input.Notes);
    }

    protected override async Task<IQueryable<MaintenanceRequest>> CreateFilteredQueryAsync(GetMaintenanceListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        var equipment = input.EquipmentNo?.Trim().ToUpperInvariant();
        return query
            .WhereIf(!equipment.IsNullOrEmpty(), r => r.EquipmentNo == equipment)
            .WhereIf(!filter.IsNullOrEmpty(), r => r.No.ToLower().Contains(filter) || r.EquipmentNo.ToLower().Contains(filter) || (r.Subject != null && r.Subject.ToLower().Contains(filter)));
    }

    protected override IQueryable<MaintenanceRequest> ApplyDefaultSorting(IQueryable<MaintenanceRequest> query) => query.OrderByDescending(r => r.No);
}
