using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.CostAccounting;

public class CostAccountingSetupDto
{
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostCenterDimension { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostObjectDimension { get; set; }

    public long LastGLEntryNoTransferred { get; set; }
}

public interface ICostAccountingSetupAppService : IApplicationService
{
    Task<CostAccountingSetupDto> GetAsync();

    Task<CostAccountingSetupDto> UpdateAsync(CostAccountingSetupDto input);
}

public class CostTypeDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public CostTypeKind Type { get; set; }
    public string GLAccountRange { get; set; }
    public string Totaling { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateCostTypeDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public CostTypeKind Type { get; set; }

    [StringLength(250)]
    public string GLAccountRange { get; set; }

    [StringLength(250)]
    public string Totaling { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostCenterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostObjectCode { get; set; }

    public bool Blocked { get; set; }
}

public interface ICostTypeAppService : ICrudAppService<CostTypeDto, Guid, GetCodeTableListInput, CreateUpdateCostTypeDto> { }

public class CostCenterDto : CodeTableDto
{
    public bool Blocked { get; set; }
}

public class CreateUpdateCostCenterDto : CreateUpdateCodeTableDto
{
    public bool Blocked { get; set; }
}

public interface ICostCenterAppService : ICrudAppService<CostCenterDto, Guid, GetCodeTableListInput, CreateUpdateCostCenterDto> { }

public class CostObjectDto : CodeTableDto
{
    public bool Blocked { get; set; }
}

public class CreateUpdateCostObjectDto : CreateUpdateCodeTableDto
{
    public bool Blocked { get; set; }
}

public interface ICostObjectAppService : ICrudAppService<CostObjectDto, Guid, GetCodeTableListInput, CreateUpdateCostObjectDto> { }

public class CostEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string CostTypeNo { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public long GLEntryNo { get; set; }
    public string GLAccountNo { get; set; }
    public string AllocationCode { get; set; }
    public long RegisterNo { get; set; }
}

public class GetCostEntryListInput : ErpPagedListInput
{
    public string CostTypeNo { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
}

public interface ICostEntryAppService : IReadOnlyAppService<CostEntryDto, Guid, GetCostEntryListInput> { }

public class CostRegisterDto : EntityDto<Guid>
{
    public long No { get; set; }
    public CostRegisterSource Source { get; set; }
    public DateTime PostingDate { get; set; }
    public long FromEntryNo { get; set; }
    public long ToEntryNo { get; set; }
    public int EntryCount { get; set; }
    public decimal DebitAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface ICostRegisterAppService : IReadOnlyAppService<CostRegisterDto, Guid, ErpPagedListInput> { }

public class CostAllocationSourceDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public string CostTypeRange { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
    public string CreditToCostTypeNo { get; set; }
    public bool Blocked { get; set; }
    public decimal TotalShare { get; set; }
}

public class CreateUpdateCostAllocationSourceDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(1, 99)]
    public int Level { get; set; } = 1;

    [Required]
    [StringLength(250)]
    public string CostTypeRange { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostCenterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostObjectCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CreditToCostTypeNo { get; set; }

    public bool Blocked { get; set; }
}

public interface ICostAllocationSourceAppService
    : ICrudAppService<CostAllocationSourceDto, Guid, GetCodeTableListInput, CreateUpdateCostAllocationSourceDto> { }

public class CostAllocationTargetDto : EntityDto<Guid>
{
    public Guid CostAllocationSourceId { get; set; }
    public int LineNo { get; set; }
    public string CostTypeNo { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
    public decimal Share { get; set; }
}

public class CreateUpdateCostAllocationTargetDto
{
    public Guid CostAllocationSourceId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CostTypeNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostCenterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CostObjectCode { get; set; }

    public decimal Share { get; set; }
}

public class GetCostAllocationTargetListInput : ErpPagedListInput
{
    public Guid? CostAllocationSourceId { get; set; }
}

public interface ICostAllocationTargetAppService
    : ICrudAppService<CostAllocationTargetDto, Guid, GetCostAllocationTargetListInput, CreateUpdateCostAllocationTargetDto> { }

public class AllocateCostsInput
{
    [Range(1, 99)]
    public int FromLevel { get; set; } = 1;

    [Range(1, 99)]
    public int ToLevel { get; set; } = 99;

    public DateTime? PostingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }
}

public class GetCostBalancesInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool ByCostObject { get; set; }
}

public class CostBalanceDto
{
    public string CostTypeNo { get; set; }
    public string CostTypeName { get; set; }
    public string CostCenterCode { get; set; }
    public string CostObjectCode { get; set; }
    public decimal Amount { get; set; }
}

public interface ICostAccountingAppService : IApplicationService
{
    /// <summary>Null when there was nothing to transfer.</summary>
    Task<CostRegisterDto> TransferFromGeneralLedgerAsync();

    Task<CostRegisterDto> AllocateAsync(AllocateCostsInput input);

    Task<List<CostBalanceDto>> GetBalancesAsync(GetCostBalancesInput input);
}
