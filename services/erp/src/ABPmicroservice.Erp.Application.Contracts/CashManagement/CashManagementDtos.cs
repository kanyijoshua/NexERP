using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.CashManagement;

public class BankAccountPostingGroupDto : PostingGroupDto
{
    public string GLAccountNo { get; set; }
}

public class CreateUpdateBankAccountPostingGroupDto : CreateUpdatePostingGroupDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }
}

public class BankAccountDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public string BankAccountNo { get; set; }
    public string BankBranchNo { get; set; }
    public string Iban { get; set; }
    public string SwiftCode { get; set; }
    public string CurrencyCode { get; set; }
    public string BankAccPostingGroup { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PhoneNo { get; set; }
    public string Contact { get; set; }
    public decimal Balance { get; set; }
    public decimal BalanceLcy { get; set; }
    public bool Blocked { get; set; }

    public string Name2 { get; set; }
    public string Address2 { get; set; }
    public string PostCode { get; set; }
    public string County { get; set; }
    public string CountryRegionCode { get; set; }
    public string Email { get; set; }
    public string FaxNo { get; set; }
    public string HomePage { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string OurContactCode { get; set; }
    public decimal MinBalance { get; set; }
    public string LastStatementNo { get; set; }
    public decimal BalanceLastStatement { get; set; }
    public string LastPaymentStatementNo { get; set; }
    public string LastCheckNo { get; set; }
    public string TransitNo { get; set; }
    public string BankClearingCode { get; set; }
}

public class CreateUpdateBankAccountDto
{
    /// <summary>Blank takes the next number of the General Ledger Setup's Bank Account Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BankBranchNo { get; set; }

    [StringLength(ErpDomainConsts.MaxIbanLength)]
    public string Iban { get; set; }

    [StringLength(ErpDomainConsts.MaxSwiftCodeLength)]
    public string SwiftCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string BankAccPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Contact { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(20)]
    public string PostCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(80)]
    public string HomePage { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(20)]
    public string OurContactCode { get; set; }

    public decimal MinBalance { get; set; }

    [StringLength(20)]
    public string LastStatementNo { get; set; }

    public decimal BalanceLastStatement { get; set; }

    [StringLength(20)]
    public string LastPaymentStatementNo { get; set; }

    [StringLength(20)]
    public string LastCheckNo { get; set; }

    [StringLength(20)]
    public string TransitNo { get; set; }

    [StringLength(50)]
    public string BankClearingCode { get; set; }
}

public class GetBankAccountListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

public class BankAccountLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountNo { get; set; }
    public DateTime PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountLcy { get; set; }
    public string CurrencyCode { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool Open { get; set; }
    public bool Reversed { get; set; }
}

public class GetBankAccountLedgerEntryListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public Guid? BankAccountId { get; set; }
}

public class PaymentMethodDto : CodeTableDto
{
    public GenJournalAccountType? BalAccountType { get; set; }
    public string BalAccountNo { get; set; }

    public bool DirectDebit { get; set; }
    public string DirectDebitPmtTermsCode { get; set; }
    public string PmtExportLineDefinition { get; set; }
}

public class CreateUpdatePaymentMethodDto : CreateUpdateCodeTableDto
{
    /// <summary>G/L Account or Bank Account; blank for no balancing account.</summary>
    public GenJournalAccountType? BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }

    public bool DirectDebit { get; set; }

    [StringLength(10)]
    public string DirectDebitPmtTermsCode { get; set; }

    [StringLength(20)]
    public string PmtExportLineDefinition { get; set; }
}

public interface IBankAccountPostingGroupAppService
    : ICrudAppService<BankAccountPostingGroupDto, Guid, GetCodeTableListInput, CreateUpdateBankAccountPostingGroupDto, CreateUpdateBankAccountPostingGroupDto> { }

public interface IBankAccountAppService
    : ICrudAppService<BankAccountDto, Guid, GetBankAccountListInput, CreateUpdateBankAccountDto, CreateUpdateBankAccountDto>
{
    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface IBankAccountLedgerEntryAppService
    : IReadOnlyAppService<BankAccountLedgerEntryDto, Guid, GetBankAccountLedgerEntryListInput> { }

public interface IPaymentMethodAppService
    : ICrudAppService<PaymentMethodDto, Guid, GetCodeTableListInput, CreateUpdatePaymentMethodDto, CreateUpdatePaymentMethodDto> { }
