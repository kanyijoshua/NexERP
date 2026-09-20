using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public class GenJournalTemplateDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string Description { get; set; }

    public GenJournalTemplateType Type { get; set; }

    public bool Recurring { get; set; }

    public string SourceCode { get; set; }

    public string NoSeriesCode { get; set; }

    /// <summary>How many batches hang off the template, so the UI can warn before a delete.</summary>
    public int BatchCount { get; set; }
}

public class CreateUpdateGenJournalTemplateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public GenJournalTemplateType Type { get; set; } = GenJournalTemplateType.General;

    public bool Recurring { get; set; }

    [StringLength(ErpDomainConsts.MaxSourceCodeLength)]
    public string SourceCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string NoSeriesCode { get; set; }
}

public class GenJournalBatchDto : EntityDto<Guid>
{
    public string JournalTemplateName { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ReasonCode { get; set; }

    public string NoSeriesCode { get; set; }

    public GenJournalAccountType? BalAccountType { get; set; }

    public string BalAccountNo { get; set; }

    /// <summary>True when the batch's template is a recurring one; the lines then show their frequency.</summary>
    public bool Recurring { get; set; }

    public int LineCount { get; set; }

    /// <summary>Sum of the lines that do not carry their own balancing account.</summary>
    public decimal Balance { get; set; }
}

public class CreateGenJournalBatchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string JournalTemplateName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxReasonCodeLength)]
    public string ReasonCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string NoSeriesCode { get; set; }

    public GenJournalAccountType? BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }
}

public class UpdateGenJournalBatchDto
{
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxReasonCodeLength)]
    public string ReasonCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string NoSeriesCode { get; set; }

    public GenJournalAccountType? BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }
}

public class GenJournalLineDto : EntityDto<Guid>
{
    public Guid GenJournalBatchId { get; set; }

    public int LineNo { get; set; }

    public DateTime PostingDate { get; set; }

    public DateTime DocumentDate { get; set; }

    public GLEntryDocumentType DocumentType { get; set; }

    public string DocumentNo { get; set; }

    public string ExternalDocumentNo { get; set; }

    public GenJournalAccountType AccountType { get; set; }

    public string AccountNo { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }

    public GenJournalAccountType? BalAccountType { get; set; }

    public string BalAccountNo { get; set; }

    public RecurringMethod RecurringMethod { get; set; }

    public string RecurringFrequency { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string AppliesToDocNo { get; set; }

    public string Comment { get; set; }
}

public class CreateUpdateGenJournalLineDto
{
    public Guid GenJournalBatchId { get; set; }

    public DateTime PostingDate { get; set; }

    public DateTime? DocumentDate { get; set; }

    public GLEntryDocumentType DocumentType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string ExternalDocumentNo { get; set; }

    public GenJournalAccountType AccountType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Amount { get; set; }

    public GenJournalAccountType? BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }

    public RecurringMethod RecurringMethod { get; set; } = RecurringMethod.None;

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string RecurringFrequency { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string AppliesToDocNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCommentLength)]
    public string Comment { get; set; }
}

public class GenJournalPostingResultDto
{
    public long RegisterNo { get; set; }

    public long TransactionNo { get; set; }

    public int PostedLineCount { get; set; }

    public int PostedDocumentCount { get; set; }

    public int RecurringLineCount { get; set; }

    public int ReversingLineCount { get; set; }
}

/// <summary>Messages a batch would fail on. An empty list means it is ready to post.</summary>
public class JournalCheckResultDto
{
    public bool IsValid => Messages.Count == 0;

    public List<string> Messages { get; set; } = new();
}

/// <summary>One entry a posting would write, shown before anything is committed.</summary>
public class PostingPreviewLineDto
{
    /// <summary>"GLEntry", "CustomerLedgerEntry" or "VendorLedgerEntry".</summary>
    public string Ledger { get; set; }

    public DateTime PostingDate { get; set; }

    public string DocumentNo { get; set; }

    public string AccountNo { get; set; }

    public string Description { get; set; }

    public decimal DebitAmount { get; set; }

    public decimal CreditAmount { get; set; }
}

public class PostingPreviewDto
{
    public List<PostingPreviewLineDto> Lines { get; set; } = new();

    /// <summary>Sum of the G/L entries. Anything but zero means the run would not balance.</summary>
    public decimal GLBalance { get; set; }
}

public class GetGenJournalBatchesInput
{
    /// <summary>Blank returns the batches of every template.</summary>
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string JournalTemplateName { get; set; }
}

public class GLRegisterDto : EntityDto<Guid>
{
    public long No { get; set; }

    public long TransactionNo { get; set; }

    public DateTime PostingDate { get; set; }

    public DateTime CreationTime { get; set; }

    public string UserName { get; set; }

    public string SourceCode { get; set; }

    public string JournalBatchName { get; set; }

    public long FromEntryNo { get; set; }

    public long ToEntryNo { get; set; }

    public bool Reversed { get; set; }

    public long ReversedByRegisterNo { get; set; }

    public long ReversedRegisterNo { get; set; }

    public bool IsReversible { get; set; }
}

public class GetGLRegistersInput : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    /// <summary>Hides registers that have already been reversed, and reversals themselves.</summary>
    public bool OnlyReversible { get; set; }
}

public class ReverseRegisterInput
{
    public long RegisterNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class ReversalResultDto
{
    public long ReversedRegisterNo { get; set; }

    public long ReversalRegisterNo { get; set; }

    public int GLEntryCount { get; set; }

    public int CustomerEntryCount { get; set; }

    public int VendorEntryCount { get; set; }
}

public class StandardJournalDto : EntityDto<Guid>
{
    public string JournalTemplateName { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public int LineCount { get; set; }
}

public class SaveStandardJournalInput
{
    public Guid BatchId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class ApplyStandardJournalInput
{
    public Guid StandardJournalId { get; set; }

    public Guid BatchId { get; set; }

    public DateTime PostingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }
}

public class GetStandardJournalsInput
{
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string JournalTemplateName { get; set; }
}

/// <summary>Journal templates. Mirrors Business Central table 80 "Gen. Journal Template".</summary>
public interface IJournalTemplateAppService : IApplicationService
{
    Task<ListResultDto<GenJournalTemplateDto>> GetListAsync();

    Task<GenJournalTemplateDto> CreateAsync(CreateUpdateGenJournalTemplateDto input);

    Task<GenJournalTemplateDto> UpdateAsync(Guid id, CreateUpdateGenJournalTemplateDto input);

    Task DeleteAsync(Guid id);
}

/// <summary>
/// The general journal: batches, their lines, and posting them.
/// Mirrors Business Central page 39 together with codeunits 11 and 13.
/// </summary>
public interface IGeneralJournalAppService : IApplicationService
{
    Task<ListResultDto<GenJournalBatchDto>> GetBatchesAsync(GetGenJournalBatchesInput input);

    Task<GenJournalBatchDto> CreateBatchAsync(CreateGenJournalBatchDto input);

    Task<GenJournalBatchDto> UpdateBatchAsync(Guid id, UpdateGenJournalBatchDto input);

    Task DeleteBatchAsync(Guid id);

    Task<ListResultDto<GenJournalLineDto>> GetLinesAsync(Guid batchId);

    Task<GenJournalLineDto> CreateLineAsync(CreateUpdateGenJournalLineDto input);

    Task<GenJournalLineDto> UpdateLineAsync(Guid id, CreateUpdateGenJournalLineDto input);

    Task DeleteLineAsync(Guid id);

    /// <summary>Validates without posting. Routed as POST /api/erp/general-journal/check/{batchId}.</summary>
    Task<JournalCheckResultDto> CheckAsync(Guid batchId);

    /// <summary>
    /// Posts inside a transaction that is rolled back, and returns the entries that would have
    /// been written. Mirrors Business Central's Preview Posting.
    /// </summary>
    Task<PostingPreviewDto> PreviewAsync(Guid batchId);

    /// <summary>Posts every line of the batch. Routed as POST /api/erp/general-journal/run-posting/{batchId}.</summary>
    Task<GenJournalPostingResultDto> RunPostingAsync(Guid batchId);
}

/// <summary>
/// G/L registers and reversal. Mirrors Business Central page 116 and codeunit 17.
/// </summary>
public interface IGLRegisterAppService : IApplicationService
{
    Task<PagedResultDto<GLRegisterDto>> GetListAsync(GetGLRegistersInput input);

    /// <summary>The entries a register wrote, for the navigate action.</summary>
    Task<ListResultDto<PostingPreviewLineDto>> GetEntriesAsync(long registerNo);

    /// <summary>Routed as POST /api/erp/g-l-register/run-reversal.</summary>
    Task<ReversalResultDto> RunReversalAsync(ReverseRegisterInput input);
}

/// <summary>
/// Saved sets of journal lines. Mirrors Business Central tables 750 and 751.
/// </summary>
public interface IStandardJournalAppService : IApplicationService
{
    Task<ListResultDto<StandardJournalDto>> GetListAsync(GetStandardJournalsInput input);

    /// <summary>Stores the current lines of a batch under a code.</summary>
    Task<StandardJournalDto> SaveFromBatchAsync(SaveStandardJournalInput input);

    /// <summary>Copies the saved lines into a batch, dated and numbered as asked.</summary>
    Task<ListResultDto<GenJournalLineDto>> ApplyToBatchAsync(ApplyStandardJournalInput input);

    Task DeleteAsync(Guid id);
}
