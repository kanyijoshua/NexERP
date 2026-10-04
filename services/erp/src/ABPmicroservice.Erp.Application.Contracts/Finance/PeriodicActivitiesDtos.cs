using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public class VatReturnInput
{
    public DateTime StartingDate { get; set; }

    public DateTime EndingDate { get; set; }

    public VatEntrySelection Selection { get; set; } = VatEntrySelection.OpenAndClosed;
}

public class VatReturnLineDto
{
    public VatEntryType Type { get; set; }
    public string VatIdentifier { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public decimal VatPercent { get; set; }
    public decimal Base { get; set; }
    public decimal Amount { get; set; }
}

public class VatReturnDto
{
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
    public VatEntrySelection Selection { get; set; }
    public List<VatReturnLineDto> Lines { get; set; } = new();
    public decimal OutputVatOnSales { get; set; }
    public decimal OutputVatOnReverseCharge { get; set; }
    public decimal TotalOutputVat { get; set; }
    public decimal InputVat { get; set; }
    public decimal NetVatDue { get; set; }
    public decimal SalesExcludingVat { get; set; }
    public decimal PurchasesExcludingVat { get; set; }
}

public class VatSettlementInput
{
    public DateTime? StartingDate { get; set; }

    public DateTime EndingDate { get; set; }

    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SettlementAccountNo { get; set; }
}

public class VatSettlementLineDto
{
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public string VatIdentifier { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public VatEntryType Type { get; set; }
    public decimal VatPercent { get; set; }
    public int EntryCount { get; set; }
    public decimal Base { get; set; }
    public decimal Amount { get; set; }
}

public class VatSettlementDto
{
    public List<VatSettlementLineDto> Lines { get; set; } = new();
    public decimal NetVatPayable { get; set; }
    public bool Posted { get; set; }
    public long RegisterNo { get; set; }
}

public class ExchRateAdjustmentInput
{
    public DateTime EndingDate { get; set; }

    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }

    public bool AdjustCustomers { get; set; } = true;

    public bool AdjustVendors { get; set; } = true;

    public bool AdjustBankAccounts { get; set; } = true;
}

public class ExchRateAdjustmentLineDto
{
    public ExchRateAdjmtAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public string CurrencyCode { get; set; }
    public decimal CurrencyFactor { get; set; }
    public decimal Base { get; set; }
    public decimal OldAmountLcy { get; set; }
    public decimal NewAmountLcy { get; set; }
    public decimal Difference { get; set; }
}

public class ExchRateAdjustmentDto
{
    public List<ExchRateAdjustmentLineDto> Lines { get; set; } = new();
    public decimal TotalGains { get; set; }
    public decimal TotalLosses { get; set; }
    public bool Posted { get; set; }
    public long RegisterNo { get; set; }
}

public class ExchRateAdjmtRegisterDto : EntityDto<Guid>
{
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public ExchRateAdjmtAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public string CurrencyCode { get; set; }
    public decimal CurrencyFactor { get; set; }
    public decimal AdjustedBase { get; set; }
    public decimal AdjustedBaseLcy { get; set; }
    public decimal AdjustedAmount { get; set; }
    public long GLRegisterNo { get; set; }
}

public class GetExchRateAdjmtRegisterListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}

/// <summary>A customer or vendor ledger entry, as the ledger entry pages show it.</summary>
public class PartyLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string PartyNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountLcy { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal RemainingAmountLcy { get; set; }
    public DateTime DueDate { get; set; }
    public bool Open { get; set; }
    public long ClosedByEntryNo { get; set; }
    public bool Reversed { get; set; }
}

public class GetPartyLedgerEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the document number or the customer/vendor number.</summary>
    public string Filter { get; set; }

    public string PartyNo { get; set; }

    public bool OnlyOpen { get; set; }
}

/// <summary>The VAT statement (return) and the VAT settlement.</summary>
public interface IVatReportingAppService : IApplicationService
{
    Task<VatReturnDto> CalculateReturnAsync(VatReturnInput input);

    Task<VatSettlementDto> CalculateSettlementAsync(VatSettlementInput input);

    Task<VatSettlementDto> SettleAsync(VatSettlementInput input);
}

/// <summary>Exchange rate adjustment and its register.</summary>
public interface IExchRateAdjustmentAppService : IApplicationService
{
    Task<ExchRateAdjustmentDto> CalculateAsync(ExchRateAdjustmentInput input);

    Task<ExchRateAdjustmentDto> AdjustAsync(ExchRateAdjustmentInput input);

    Task<PagedResultDto<ExchRateAdjmtRegisterDto>> GetRegistersAsync(GetExchRateAdjmtRegisterListInput input);
}

public interface ICustomerLedgerEntryAppService : IApplicationService
{
    Task<PagedResultDto<PartyLedgerEntryDto>> GetListAsync(GetPartyLedgerEntryListInput input);
}

public interface IVendorLedgerEntryAppService : IApplicationService
{
    Task<PagedResultDto<PartyLedgerEntryDto>> GetListAsync(GetPartyLedgerEntryListInput input);
}
