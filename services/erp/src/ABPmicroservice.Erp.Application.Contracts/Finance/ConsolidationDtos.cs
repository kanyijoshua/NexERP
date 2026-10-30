using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public class BusinessUnitDto : CodeTableDto
{
    public Guid SubsidiaryCompanyId { get; set; }
    public bool Consolidate { get; set; }
    public decimal ConsolidationPct { get; set; }
    public string CurrencyCode { get; set; }
    public string ResidualAccountNo { get; set; }
    public DateTime? LastRunStartingDate { get; set; }
    public DateTime? LastRunEndingDate { get; set; }
}

public class CreateUpdateBusinessUnitDto : CreateUpdateCodeTableDto
{
    public Guid SubsidiaryCompanyId { get; set; }
    public bool Consolidate { get; set; } = true;

    [Range(0, 100)]
    public decimal ConsolidationPct { get; set; } = 100m;

    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ResidualAccountNo { get; set; }
}

public class ConsolidationPeriodInput
{
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
}

public class RunConsolidationInput : ConsolidationPeriodInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>One business unit; blank runs every unit set to consolidate.</summary>
    public Guid? BusinessUnitId { get; set; }
}

public class ConsolidationLineDto
{
    public string BusinessUnitCode { get; set; }
    public string SubsidiaryAccountNo { get; set; }
    public string ConsolidationAccountNo { get; set; }
    public ConsolidationTranslationMethod TranslationMethod { get; set; }
    public decimal SubsidiaryAmount { get; set; }
    public decimal Amount { get; set; }
}

public interface IBusinessUnitAppService : ICrudAppService<BusinessUnitDto, Guid, GetCodeTableListInput, CreateUpdateBusinessUnitDto>
{
    Task<List<ConsolidationLineDto>> GetPreviewAsync(Guid id, ConsolidationPeriodInput input);

    Task<List<ConsolidationLineDto>> RunConsolidationAsync(RunConsolidationInput input);
}
