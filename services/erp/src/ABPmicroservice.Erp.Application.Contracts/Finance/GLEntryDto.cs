using System;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Finance;

public class GLEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public Guid GLAccountId { get; set; }
    public string GLAccountNo { get; set; }
    public DateTime PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string SourceNo { get; set; }
    public string GenBusPostingGroup { get; set; }

    public string GLAccountName { get; set; }
    public GeneralPostingType GenPostingType { get; set; }
    public string GenProdPostingGroup { get; set; }
    public GenJournalAccountType BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
    public decimal VATAmount { get; set; }
    public string VATBusPostingGroup { get; set; }
    public string VATProdPostingGroup { get; set; }
    public string ExternalDocumentNo { get; set; }
    public GLEntrySourceType SourceType { get; set; }
    public string UserId { get; set; }
    public string JournalBatchName { get; set; }
    public decimal Quantity { get; set; }
    public decimal AdditionalCurrencyAmount { get; set; }
    public string JobNo { get; set; }
    public string BusinessUnitCode { get; set; }
}

public class GetGLEntryListInput : ErpPagedListInput
{
    public Guid? GLAccountId { get; set; }
    public string DocumentNo { get; set; }
    public string ExternalDocumentNo { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
