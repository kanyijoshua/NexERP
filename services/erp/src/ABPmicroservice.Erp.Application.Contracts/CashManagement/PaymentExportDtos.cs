using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.CashManagement;

public class SuggestVendorPaymentsInput
{
    public DateTime? PostingDate { get; set; }

    /// <summary>Invoices due on or before it are paid; blank is the posting date.</summary>
    public DateTime? LastDueDate { get; set; }

    public bool UsePaymentDiscounts { get; set; }

    /// <summary>Vendor numbers separated by commas; blank pays every vendor.</summary>
    [StringLength(1000)]
    public string VendorNos { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BankAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }
}

public class SuggestVendorPaymentsResultDto
{
    public int LinesAdded { get; set; }
}

public class ExportPaymentsInput
{
    public PaymentFileFormat Format { get; set; } = PaymentFileFormat.Iso20022CreditTransfer;
}

/// <summary>A file for the bank; the content is the file's text.</summary>
public class BankFileDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Content { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public interface IPaymentJournalAppService : IApplicationService
{
    Task<SuggestVendorPaymentsResultDto> SuggestVendorPaymentsAsync(Guid id, SuggestVendorPaymentsInput input);

    Task<BankFileDto> ExportAsync(Guid id, ExportPaymentsInput input);
}

public class DirectDebitMandateDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerBankAccountCode { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DirectDebitMandateType Type { get; set; }
    public int ExpectedUses { get; set; }
    public int UsedCount { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateDirectDebitMandateDto
{
    [Required]
    [StringLength(35)]
    public string Code { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CustomerBankAccountCode { get; set; }

    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DirectDebitMandateType Type { get; set; }

    [Range(0, 9999)]
    public int ExpectedUses { get; set; }

    public bool Blocked { get; set; }
}

public class GetDirectDebitMandateListInput : ErpPagedListInput
{
    public string CustomerNo { get; set; }
    public string Filter { get; set; }
}

public interface IDirectDebitMandateAppService
    : ICrudAppService<DirectDebitMandateDto, Guid, GetDirectDebitMandateListInput, CreateUpdateDirectDebitMandateDto> { }

public class DirectDebitCollectionDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string BankAccountNo { get; set; }
    public string CreditorIdentifier { get; set; }
    public DateTime CreatedDate { get; set; }
    public DirectDebitCollectionStatus Status { get; set; }
    public int EntryCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class DirectDebitCollectionEntryDto : EntityDto<Guid>
{
    public Guid DirectDebitCollectionId { get; set; }
    public int LineNo { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public string DocumentNo { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public string MandateCode { get; set; }
    public DirectDebitSequenceType SequenceType { get; set; }
}

public class GetDirectDebitCollectionListInput : ErpPagedListInput
{
    public DirectDebitCollectionStatus? Status { get; set; }
}

public class GetDirectDebitCollectionEntryListInput : ErpPagedListInput
{
    public Guid? DirectDebitCollectionId { get; set; }
}

public class MakeDirectDebitCollectionInput
{
    public DateTime FromDueDate { get; set; }
    public DateTime ToDueDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BankAccountNo { get; set; }

    [StringLength(35)]
    public string CreditorIdentifier { get; set; }
}

public class MakeDirectDebitReceiptLinesInput
{
    /// <summary>The general journal batch the cash receipt lines go to.</summary>
    public Guid GenJournalBatchId { get; set; }

    public DateTime? PostingDate { get; set; }
}

public class DirectDebitReceiptLinesResultDto
{
    public int LinesAdded { get; set; }
}

public interface IDirectDebitCollectionAppService : IReadOnlyAppService<DirectDebitCollectionDto, Guid, GetDirectDebitCollectionListInput>
{
    Task<DirectDebitCollectionDto> MakeAsync(MakeDirectDebitCollectionInput input);

    Task<BankFileDto> ExportAsync(Guid id);

    Task<DirectDebitReceiptLinesResultDto> MakeReceiptLinesAsync(Guid id, MakeDirectDebitReceiptLinesInput input);

    Task<DirectDebitCollectionDto> CloseAsync(Guid id);

    Task DeleteAsync(Guid id);
}

public interface IDirectDebitCollectionEntryAppService
    : IReadOnlyAppService<DirectDebitCollectionEntryDto, Guid, GetDirectDebitCollectionEntryListInput> { }
