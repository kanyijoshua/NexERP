using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Fleet;

/// <summary>Vehicle: a car or truck of the fleet, its driver and its odometer.</summary>
public class Vehicle : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string LicensePlate { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string Vin { get; private set; }
    public string FuelType { get; private set; }
    public string DriverEmployeeNo { get; private set; }
    public string FixedAssetNo { get; private set; }
    public DateTime? AcquisitionDate { get; private set; }

    /// <summary>The highest reading logged, in kilometres.</summary>
    public decimal Odometer { get; private set; }

    public VehicleStatus Status { get; private set; }

    protected Vehicle() { }

    public Vehicle(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string licensePlate, string make, string model, int year, string vin, string fuelType, string driverEmployeeNo, string fixedAssetNo, DateTime? acquisitionDate, VehicleStatus status)
    {
        LicensePlate = Check.NotNullOrWhiteSpace(licensePlate, nameof(licensePlate), 20).Trim().ToUpperInvariant();
        Make = Check.Length(make, nameof(make), 50);
        Model = Check.Length(model, nameof(model), 50);
        Year = year;
        Vin = Check.Length(vin?.Trim(), nameof(vin), 30);
        FuelType = Check.Length(fuelType, nameof(fuelType), 20);
        DriverEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(driverEmployeeNo, nameof(driverEmployeeNo), ErpDomainConsts.MaxNoLength));
        FixedAssetNo = CodeTableEntity.NormalizeCode(Check.Length(fixedAssetNo, nameof(fixedAssetNo), ErpDomainConsts.MaxNoLength));
        AcquisitionDate = acquisitionDate?.Date;
        Status = status;
    }

    /// <summary>A reading below the last one is refused: odometers only go up.</summary>
    internal void Read(decimal odometer)
    {
        if (odometer < Odometer)
        {
            throw new BusinessException(ErpErrorCodes.Fleet.OdometerBackwards).WithData("no", No).WithData("odometer", Odometer).WithData("value", odometer);
        }

        Odometer = odometer;
    }
}

/// <summary>Vehicle Log Entry: an odometer reading, a fill-up or a service, with what it cost.</summary>
public class VehicleLogEntry : CompanyEntity
{
    public string VehicleNo { get; private set; }
    public DateTime Date { get; private set; }
    public VehicleLogType Type { get; private set; }
    public string Description { get; private set; }
    public decimal Odometer { get; private set; }
    public decimal Liters { get; private set; }
    public decimal Cost { get; private set; }
    public string VendorNo { get; private set; }

    protected VehicleLogEntry() { }

    public VehicleLogEntry(Guid id, string vehicleNo, DateTime date, VehicleLogType type, string description, decimal odometer, decimal liters, decimal cost, string vendorNo)
        : base(id)
    {
        VehicleNo = vehicleNo;
        Date = date.Date;
        Type = type;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Odometer = Math.Max(0m, odometer);
        Liters = Math.Max(0m, liters);
        Cost = Math.Max(0m, cost);
        VendorNo = vendorNo.IsNullOrWhiteSpace() ? null : Check.Length(vendorNo.Trim(), nameof(vendorNo), ErpDomainConsts.MaxNoLength);
    }
}

/// <summary>Vehicle Contract: a lease, insurance or service agreement with a recurring cost and an end date.</summary>
public class VehicleContract : CompanyEntity
{
    public string VehicleNo { get; private set; }
    public VehicleContractType Type { get; private set; }
    public string VendorNo { get; private set; }
    public string Reference { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal RecurringCost { get; private set; }
    public ContractCostFrequency CostFrequency { get; private set; }

    protected VehicleContract() { }

    public VehicleContract(Guid id, string vehicleNo)
        : base(id)
    {
        VehicleNo = vehicleNo;
    }

    public void Set(VehicleContractType type, string vendorNo, string reference, DateTime startDate, DateTime? endDate, decimal recurringCost, ContractCostFrequency costFrequency)
    {
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Fleet.ContractDates).WithData("no", VehicleNo);
        }

        Type = type;
        VendorNo = vendorNo.IsNullOrWhiteSpace() ? null : Check.Length(vendorNo.Trim(), nameof(vendorNo), ErpDomainConsts.MaxNoLength);
        Reference = Check.Length(reference, nameof(reference), 50);
        StartDate = startDate.Date;
        EndDate = endDate?.Date;
        RecurringCost = Math.Max(0m, recurringCost);
        CostFrequency = costFrequency;
    }

    /// <summary>What the contract cost over a period: its recurring cost for every month (or year) the two overlap.</summary>
    public decimal CostBetween(DateTime from, DateTime to)
    {
        var start = StartDate > from.Date ? StartDate : from.Date;
        var end = EndDate.HasValue && EndDate.Value < to.Date ? EndDate.Value : to.Date;
        if (end < start)
        {
            return 0m;
        }

        var months = (end.Year - start.Year) * 12 + end.Month - start.Month + 1;
        return CostFrequency == ContractCostFrequency.Monthly ? RecurringCost * months : Math.Round(RecurringCost * months / 12m, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>What a vehicle cost to run over a period.</summary>
public sealed class VehicleCost
{
    public string VehicleNo { get; init; }
    public string LicensePlate { get; init; }
    public decimal FuelCost { get; init; }
    public decimal ServiceCost { get; init; }
    public decimal ContractCost { get; init; }
    public decimal TotalCost { get; init; }
    public decimal Liters { get; init; }
    public decimal Distance { get; init; }

    /// <summary>Total cost per kilometre driven; zero when nothing was driven.</summary>
    public decimal CostPerKm { get; init; }
}

/// <summary>Logs readings and costs against vehicles and works out what each one costs to run.</summary>
public class FleetManager : DomainService
{
    private readonly IRepository<Vehicle, Guid> _vehicles;
    private readonly IRepository<VehicleLogEntry, Guid> _logs;
    private readonly IRepository<VehicleContract, Guid> _contracts;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public FleetManager(IRepository<Vehicle, Guid> vehicles, IRepository<VehicleLogEntry, Guid> logs, IRepository<VehicleContract, Guid> contracts)
    {
        _vehicles = vehicles;
        _logs = logs;
        _contracts = contracts;
    }

    public async Task<string> NextNoAsync() => $"V{await EntryNos.NextAsync(ErpSequenceNames.VehicleNo):D4}";

    public async Task<Vehicle> GetVehicleAsync(string no)
    {
        var normalized = no?.Trim().ToUpperInvariant();
        return await _vehicles.FirstOrDefaultAsync(v => v.No == normalized)
            ?? throw new BusinessException(ErpErrorCodes.Fleet.VehicleNotFound).WithData("no", normalized ?? "");
    }

    /// <summary>Logs an entry; a reading moves the vehicle's odometer on (a blank reading on fuel or service keeps it).</summary>
    public async Task<VehicleLogEntry> LogAsync(string vehicleNo, DateTime date, VehicleLogType type, string description, decimal odometer, decimal liters, decimal cost, string vendorNo)
    {
        var vehicle = await GetVehicleAsync(vehicleNo);
        if (odometer > 0m || type == VehicleLogType.Odometer)
        {
            vehicle.Read(odometer);
            await _vehicles.UpdateAsync(vehicle, autoSave: true);
        }

        return await _logs.InsertAsync(new VehicleLogEntry(GuidGenerator.Create(), vehicle.No, date, type, description, odometer, liters, cost, vendorNo), autoSave: true);
    }

    public async Task<List<VehicleCost>> GetCostsAsync(DateTime from, DateTime to)
    {
        var start = from.Date;
        var end = to.Date;
        var logs = (await _logs.GetListAsync(l => l.Date >= start && l.Date <= end)).GroupBy(l => l.VehicleNo).ToDictionary(g => g.Key, g => g.ToList());
        var contracts = (await _contracts.GetListAsync()).GroupBy(c => c.VehicleNo).ToDictionary(g => g.Key, g => g.ToList());
        var result = new List<VehicleCost>();
        foreach (var vehicle in (await _vehicles.GetListAsync()).OrderBy(v => v.No))
        {
            var own = logs.GetValueOrDefault(vehicle.No) ?? [];
            var readings = own.Where(l => l.Odometer > 0m).Select(l => l.Odometer).ToList();
            var distance = readings.Count < 2 ? 0m : readings.Max() - readings.Min();
            var fuel = own.Where(l => l.Type == VehicleLogType.Fuel).Sum(l => l.Cost);
            var service = own.Where(l => l.Type == VehicleLogType.Service).Sum(l => l.Cost);
            var contract = (contracts.GetValueOrDefault(vehicle.No) ?? []).Sum(c => c.CostBetween(start, end));
            var total = fuel + service + contract;
            result.Add(new VehicleCost
            {
                VehicleNo = vehicle.No,
                LicensePlate = vehicle.LicensePlate,
                FuelCost = fuel,
                ServiceCost = service,
                ContractCost = contract,
                TotalCost = total,
                Liters = own.Sum(l => l.Liters),
                Distance = distance,
                CostPerKm = distance == 0m ? 0m : Math.Round(total / distance, 4, MidpointRounding.AwayFromZero),
            });
        }

        return result;
    }
}
