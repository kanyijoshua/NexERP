using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Workflows;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.CashManagement;

// ---------------------------------------------------------------------------- Setup

public class CashManagementSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PaymentVoucherNos { get; set; }
}

public interface ICashManagementSetupAppService : IApplicationService
{
    Task<CashManagementSetupDto> GetAsync();

    Task<CashManagementSetupDto> UpdateAsync(CashManagementSetupDto input);
}

public class PaymentDeductionCodeDto : CodeTableDto
{
    public PaymentDeductionType DeductionType { get; set; }
    public decimal RatePct { get; set; }
    public string PayableAccountNo { get; set; }
}

public class CreateUpdatePaymentDeductionCodeDto : CreateUpdateCodeTableDto
{
    public PaymentDeductionType DeductionType { get; set; }

    [Range(0, 100)]
    public decimal RatePct { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PayableAccountNo { get; set; }
}

public interface IPaymentDeductionCodeAppService
    : ICrudAppService<PaymentDeductionCodeDto, Guid, GetCodeTableListInput, CreateUpdatePaymentDeductionCodeDto, CreateUpdatePaymentDeductionCodeDto> { }

public class PaymentTypeDto : CodeTableDto
{
    public GenJournalAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public decimal VatRatePct { get; set; }
    public string WithholdingTaxCode { get; set; }
    public string WithholdingVatCode { get; set; }
    public string RetentionCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePaymentTypeDto : CreateUpdateCodeTableDto
{
    public GenJournalAccountType AccountType { get; set; } = GenJournalAccountType.GLAccount;

    /// <summary>Blank leaves the account to be chosen on the voucher line.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AccountNo { get; set; }

    [Range(0, 100)]
    public decimal VatRatePct { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string WithholdingTaxCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string WithholdingVatCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string RetentionCode { get; set; }

    public bool Blocked { get; set; }
}

public interface IPaymentTypeAppService
    : ICrudAppService<PaymentTypeDto, Guid, GetCodeTableListInput, CreateUpdatePaymentTypeDto, CreateUpdatePaymentTypeDto> { }

// ---------------------------------------------------------------------------- Vouchers

public class PaymentVoucherHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime PostingDate { get; set; }
    public string PayMode { get; set; }
    public string PayingBankAccountNo { get; set; }
    public string CurrencyCode { get; set; }
    public string Payee { get; set; }
    public string OnBehalfOf { get; set; }
    public string PaymentNarration { get; set; }
    public string ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public DocumentStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalWithholdingTaxAmount { get; set; }
    public decimal TotalWithholdingVatAmount { get; set; }
    public decimal TotalRetentionAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int NoOfLines { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public string SourceType { get; set; }
    public string SourceNo { get; set; }
}

public class CreateUpdatePaymentVoucherHeaderDto
{
    /// <summary>Blank takes the next number of the Cash Management Setup's Payment Voucher Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    public DateTime DocumentDate { get; set; }

    /// <summary>Left out, the document date.</summary>
    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PayMode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PayingBankAccountNo { get; set; }

    [StringLength(100)]
    public string Payee { get; set; }

    [StringLength(100)]
    public string OnBehalfOf { get; set; }

    [StringLength(100)]
    public string PaymentNarration { get; set; }

    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string ChequeNo { get; set; }

    public DateTime? ChequeDate { get; set; }
}

public class GetPaymentVoucherListInput : ErpPagedListInput
{
    /// <summary>Matches the voucher number, the payee, the cheque number or the document it pays.</summary>
    public string Filter { get; set; }
    public string PayingBankAccountNo { get; set; }
    public DocumentStatus? Status { get; set; }
}

public class PaymentVoucherChequeInput
{
    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string ChequeNo { get; set; }

    public DateTime? ChequeDate { get; set; }
}

public interface IPaymentVoucherAppService
    : ICrudAppService<PaymentVoucherHeaderDto, Guid, GetPaymentVoucherListInput, CreateUpdatePaymentVoucherHeaderDto, CreateUpdatePaymentVoucherHeaderDto>
{
    /// <summary>Routed as POST /api/erp/payment-voucher/{id}/send-approval-request.</summary>
    Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);

    Task CancelApprovalRequestAsync(Guid id);

    Task<PaymentVoucherHeaderDto> ReleaseAsync(Guid id);

    Task<PaymentVoucherHeaderDto> ReopenAsync(Guid id);

    /// <summary>The cheque is written after approval, so it can be recorded until the voucher is posted.</summary>
    Task<PaymentVoucherHeaderDto> RecordChequeAsync(Guid id, PaymentVoucherChequeInput input);

    /// <summary>Pays the voucher. Routed as POST /api/erp/payment-voucher/{id}/run-posting.</summary>
    Task<PaymentVoucherHeaderDto> RunPostingAsync(Guid id);
}

public class PaymentVoucherLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string PaymentTypeCode { get; set; }
    public GenJournalAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public string AccountName { get; set; }
    public string Description { get; set; }
    public string AppliesToDocNo { get; set; }
    public decimal Amount { get; set; }
    public decimal VatRatePct { get; set; }
    public string WithholdingTaxCode { get; set; }
    public decimal WithholdingTaxAmount { get; set; }
    public string WithholdingVatCode { get; set; }
    public decimal WithholdingVatAmount { get; set; }
    public string RetentionCode { get; set; }
    public decimal RetentionAmount { get; set; }
    public decimal NetAmount { get; set; }
}

public class CreateUpdatePaymentVoucherLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>0 puts the line after the last one.</summary>
    public int LineNo { get; set; }

    /// <summary>Fills in whatever of the account, VAT rate and deductions the line leaves blank.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PaymentTypeCode { get; set; }

    public GenJournalAccountType? AccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string AppliesToDocNo { get; set; }

    /// <summary>The gross amount, VAT included.</summary>
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Range(0, 100)]
    public decimal? VatRatePct { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string WithholdingTaxCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string WithholdingVatCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string RetentionCode { get; set; }
}

public class GetPaymentVoucherLineListInput : ErpPagedListInput
{
    /// <summary>Matches the voucher number, the account or the description.</summary>
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
}

public interface IPaymentVoucherLineAppService
    : ICrudAppService<PaymentVoucherLineDto, Guid, GetPaymentVoucherLineListInput, CreateUpdatePaymentVoucherLineDto, CreateUpdatePaymentVoucherLineDto> { }
