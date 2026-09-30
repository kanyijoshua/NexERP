using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public class PaymentTermsDto : CodeTableDto
{
    public string DueDateCalculation { get; set; }
    public string DiscountDateCalculation { get; set; }
    public decimal DiscountPercent { get; set; }
}

public class CreateUpdatePaymentTermsDto : CreateUpdateCodeTableDto
{
    /// <summary>A date formula such as "30D" or "CM+15D".</summary>
    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string DueDateCalculation { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string DiscountDateCalculation { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
}

public class CurrencyDto : CodeTableDto
{
    public string Symbol { get; set; }
    public decimal AmountRoundingPrecision { get; set; }
    public string RealizedGainsAccountNo { get; set; }
    public string RealizedLossesAccountNo { get; set; }
    public string UnrealizedGainsAccountNo { get; set; }
    public string UnrealizedLossesAccountNo { get; set; }
}

public class CreateUpdateCurrencyDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCurrencySymbolLength)]
    public string Symbol { get; set; }

    public decimal AmountRoundingPrecision { get; set; } = 0.01m;

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string RealizedGainsAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string RealizedLossesAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string UnrealizedGainsAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string UnrealizedLossesAccountNo { get; set; }
}

public class CurrencyExchangeRateDto : FullAuditedEntityDto<Guid>
{
    public string CurrencyCode { get; set; }
    public DateTime StartingDate { get; set; }
    public decimal ExchangeRateAmount { get; set; }
    public decimal RelationalExchangeRateAmount { get; set; }
}

public class CreateUpdateCurrencyExchangeRateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }

    public DateTime StartingDate { get; set; }

    public decimal ExchangeRateAmount { get; set; } = 1m;

    public decimal RelationalExchangeRateAmount { get; set; }
}

public class AccountingPeriodDto : FullAuditedEntityDto<Guid>
{
    public DateTime StartingDate { get; set; }
    public string Name { get; set; }
    public bool NewFiscalYear { get; set; }
    public bool Closed { get; set; }
    public bool DateLocked { get; set; }
}

public class GetAccountingPeriodListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

public class NewFiscalYearDto
{
    public DateTime StartingDate { get; set; }

    [Range(1, 366)]
    public int NoOfPeriods { get; set; } = 12;

    /// <summary>A date formula, "1M" for monthly periods.</summary>
    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string PeriodLength { get; set; } = "1M";
}

public class FiscalYearClosedDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public interface IPaymentTermsAppService
    : ICrudAppService<PaymentTermsDto, Guid, GetCodeTableListInput, CreateUpdatePaymentTermsDto, CreateUpdatePaymentTermsDto> { }

public interface ICurrencyAppService
    : ICrudAppService<CurrencyDto, Guid, GetCodeTableListInput, CreateUpdateCurrencyDto, CreateUpdateCurrencyDto> { }

public interface ICurrencyExchangeRateAppService
    : ICrudAppService<CurrencyExchangeRateDto, Guid, GetCodeTableListInput, CreateUpdateCurrencyExchangeRateDto, CreateUpdateCurrencyExchangeRateDto> { }

public interface IAccountingPeriodAppService : IApplicationService
{
    Task<PagedResultDto<AccountingPeriodDto>> GetListAsync(GetAccountingPeriodListInput input);

    /// <summary>BC "Create Year". Routed as POST /api/erp/accounting-period/new-fiscal-year.</summary>
    Task<ListResultDto<AccountingPeriodDto>> NewFiscalYearAsync(NewFiscalYearDto input);

    /// <summary>BC "Close Year": closes the oldest open fiscal year.</summary>
    Task<FiscalYearClosedDto> CloseFiscalYearAsync();

    Task DeleteAsync(Guid id);
}
