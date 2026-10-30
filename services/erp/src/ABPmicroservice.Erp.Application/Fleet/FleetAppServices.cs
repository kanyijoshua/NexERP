using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Fleet;

[Authorize(ErpPermissions.Fleet.Default)]
public class VehicleAppService : ErpTableAppService<Vehicle, VehicleDto, GetFleetListInput, CreateUpdateVehicleDto>, IVehicleAppService
{
    private FleetManager Manager => LazyServiceProvider.LazyGetRequiredService<FleetManager>();

    public VehicleAppService(IRepository<Vehicle, Guid> repository)
        : base(repository, ErpPermissions.Fleet.Default) { }

    public override async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(v => v.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var vehicle = new Vehicle(GuidGenerator.Create(), no);
        Apply(vehicle, input);
        await Repository.InsertAsync(vehicle, autoSave: true);
        return await MapToGetOutputDtoAsync(vehicle);
    }

    public override async Task<VehicleDto> UpdateAsync(Guid id, CreateUpdateVehicleDto input)
    {
        await CheckUpdatePolicyAsync();
        var vehicle = await GetEntityByIdAsync(id);
        Apply(vehicle, input);
        await Repository.UpdateAsync(vehicle, autoSave: true);
        return await MapToGetOutputDtoAsync(vehicle);
    }

    public async Task<List<VehicleCostDto>> GetCostsAsync(GetVehicleCostsInput input) =>
        (await Manager.GetCostsAsync(input.FromDate, input.ToDate))
            .Select(c => new VehicleCostDto
            {
                VehicleNo = c.VehicleNo,
                LicensePlate = c.LicensePlate,
                FuelCost = c.FuelCost,
                ServiceCost = c.ServiceCost,
                ContractCost = c.ContractCost,
                TotalCost = c.TotalCost,
                Liters = c.Liters,
                Distance = c.Distance,
                CostPerKm = c.CostPerKm,
            })
            .ToList();

    private static void Apply(Vehicle vehicle, CreateUpdateVehicleDto input) =>
        vehicle.Set(input.LicensePlate, input.Make, input.Model, input.Year, input.Vin, input.FuelType, input.DriverEmployeeNo, input.FixedAssetNo, input.AcquisitionDate, input.Status);

    protected override async Task<IQueryable<Vehicle>> CreateFilteredQueryAsync(GetFleetListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), v => v.No.ToLower().Contains(filter) || v.LicensePlate.ToLower().Contains(filter) || (v.Model != null && v.Model.ToLower().Contains(filter)));
    }

    protected override IQueryable<Vehicle> ApplyDefaultSorting(IQueryable<Vehicle> query) => query.OrderBy(v => v.No);
}

[Authorize(ErpPermissions.Fleet.Default)]
public class VehicleLogEntryAppService : ErpReadOnlyAppService<VehicleLogEntry, VehicleLogEntryDto, Guid, GetFleetListInput>, IVehicleLogEntryAppService
{
    private readonly IRepository<VehicleLogEntry, Guid> _logs;

    public VehicleLogEntryAppService(IRepository<VehicleLogEntry, Guid> repository)
        : base(repository)
    {
        _logs = repository;
        GetPolicyName = ErpPermissions.Fleet.Default;
        GetListPolicyName = ErpPermissions.Fleet.Default;
    }

    [Authorize(ErpPermissions.Fleet.Create)]
    public async Task<VehicleLogEntryDto> CreateAsync(CreateVehicleLogEntryDto input)
    {
        var entry = await LazyServiceProvider.LazyGetRequiredService<FleetManager>()
            .LogAsync(input.VehicleNo, input.Date ?? Clock.Now.Date, input.Type, input.Description, input.Odometer, input.Liters, input.Cost, input.VendorNo);
        return await MapToGetOutputDtoAsync(entry);
    }

    [Authorize(ErpPermissions.Fleet.Delete)]
    public async Task DeleteAsync(Guid id) => await _logs.DeleteAsync(id, autoSave: true);

    protected override async Task<IQueryable<VehicleLogEntry>> CreateFilteredQueryAsync(GetFleetListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var vehicle = input.VehicleNo?.Trim().ToUpperInvariant();
        return query.WhereIf(!vehicle.IsNullOrEmpty(), l => l.VehicleNo == vehicle);
    }

    protected override IQueryable<VehicleLogEntry> ApplyDefaultSorting(IQueryable<VehicleLogEntry> query) => query.OrderByDescending(l => l.Date);
}

[Authorize(ErpPermissions.Fleet.Default)]
public class VehicleContractAppService : ErpTableAppService<VehicleContract, VehicleContractDto, GetFleetListInput, CreateUpdateVehicleContractDto>, IVehicleContractAppService
{
    public VehicleContractAppService(IRepository<VehicleContract, Guid> repository)
        : base(repository, ErpPermissions.Fleet.Default) { }

    public override async Task<VehicleContractDto> CreateAsync(CreateUpdateVehicleContractDto input)
    {
        await CheckCreatePolicyAsync();
        var vehicle = await LazyServiceProvider.LazyGetRequiredService<FleetManager>().GetVehicleAsync(input.VehicleNo);
        var contract = new VehicleContract(GuidGenerator.Create(), vehicle.No);
        contract.Set(input.Type, input.VendorNo, input.Reference, input.StartDate == default ? Clock.Now.Date : input.StartDate, input.EndDate, input.RecurringCost, input.CostFrequency);
        await Repository.InsertAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    public override async Task<VehicleContractDto> UpdateAsync(Guid id, CreateUpdateVehicleContractDto input)
    {
        await CheckUpdatePolicyAsync();
        var contract = await GetEntityByIdAsync(id);
        contract.Set(input.Type, input.VendorNo, input.Reference, input.StartDate == default ? contract.StartDate : input.StartDate, input.EndDate, input.RecurringCost, input.CostFrequency);
        await Repository.UpdateAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    protected override async Task<IQueryable<VehicleContract>> CreateFilteredQueryAsync(GetFleetListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var vehicle = input.VehicleNo?.Trim().ToUpperInvariant();
        return query.WhereIf(!vehicle.IsNullOrEmpty(), c => c.VehicleNo == vehicle);
    }

    protected override IQueryable<VehicleContract> ApplyDefaultSorting(IQueryable<VehicleContract> query) => query.OrderBy(c => c.VehicleNo).ThenBy(c => c.StartDate);
}
