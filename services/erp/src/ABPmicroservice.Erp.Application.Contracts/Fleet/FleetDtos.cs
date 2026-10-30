using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Fleet;

public class VehicleDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string LicensePlate { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Vin { get; set; }
    public string FuelType { get; set; }
    public string DriverEmployeeNo { get; set; }
    public string FixedAssetNo { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public decimal Odometer { get; set; }
    public VehicleStatus Status { get; set; }
}

public class CreateUpdateVehicleDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; }

    [StringLength(50)]
    public string Make { get; set; }

    [StringLength(50)]
    public string Model { get; set; }

    public int Year { get; set; }

    [StringLength(30)]
    public string Vin { get; set; }

    [StringLength(20)]
    public string FuelType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string DriverEmployeeNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string FixedAssetNo { get; set; }

    public DateTime? AcquisitionDate { get; set; }
    public VehicleStatus Status { get; set; }
}

public class GetFleetListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string VehicleNo { get; set; }
}

public class GetVehicleCostsInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class VehicleCostDto
{
    public string VehicleNo { get; set; }
    public string LicensePlate { get; set; }
    public decimal FuelCost { get; set; }
    public decimal ServiceCost { get; set; }
    public decimal ContractCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Liters { get; set; }
    public decimal Distance { get; set; }
    public decimal CostPerKm { get; set; }
}

public interface IVehicleAppService : ICrudAppService<VehicleDto, Guid, GetFleetListInput, CreateUpdateVehicleDto>
{
    Task<List<VehicleCostDto>> GetCostsAsync(GetVehicleCostsInput input);
}

public class VehicleLogEntryDto : FullAuditedEntityDto<Guid>
{
    public string VehicleNo { get; set; }
    public DateTime Date { get; set; }
    public VehicleLogType Type { get; set; }
    public string Description { get; set; }
    public decimal Odometer { get; set; }
    public decimal Liters { get; set; }
    public decimal Cost { get; set; }
    public string VendorNo { get; set; }
}

public class CreateVehicleLogEntryDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VehicleNo { get; set; }

    public DateTime? Date { get; set; }
    public VehicleLogType Type { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Odometer { get; set; }
    public decimal Liters { get; set; }
    public decimal Cost { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VendorNo { get; set; }
}

public interface IVehicleLogEntryAppService : IReadOnlyAppService<VehicleLogEntryDto, Guid, GetFleetListInput>
{
    Task<VehicleLogEntryDto> CreateAsync(CreateVehicleLogEntryDto input);

    Task DeleteAsync(Guid id);
}

public class VehicleContractDto : FullAuditedEntityDto<Guid>
{
    public string VehicleNo { get; set; }
    public VehicleContractType Type { get; set; }
    public string VendorNo { get; set; }
    public string Reference { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal RecurringCost { get; set; }
    public ContractCostFrequency CostFrequency { get; set; }
}

public class CreateUpdateVehicleContractDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VehicleNo { get; set; }

    public VehicleContractType Type { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VendorNo { get; set; }

    [StringLength(50)]
    public string Reference { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal RecurringCost { get; set; }
    public ContractCostFrequency CostFrequency { get; set; }
}

public interface IVehicleContractAppService : ICrudAppService<VehicleContractDto, Guid, GetFleetListInput, CreateUpdateVehicleContractDto> { }
