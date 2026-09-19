using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public class GenJournalBatchDto : EntityDto<Guid>
{
    public string JournalTemplateName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateGenJournalBatchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string JournalTemplateName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GenJournalLineDto : EntityDto<Guid>
{
    public Guid GenJournalBatchId { get; set; }
    public int LineNo { get; set; }
    public DateTime PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string AccountType { get; set; }
    public string AccountNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
}

public class CreateGenJournalLineDto
{
    public Guid GenJournalBatchId { get; set; }
    public DateTime PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>"G/L Account", "Customer" or "Vendor".</summary>
    [Required]
    public string AccountType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Amount { get; set; }
    public string BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }
}

public class GenJournalPostingResultDto
{
    public int PostedLineCount { get; set; }
    public int PostedDocumentCount { get; set; }
}

public interface IGeneralJournalAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/general-journal/batches.</summary>
    Task<ListResultDto<GenJournalBatchDto>> GetBatchesAsync();

    /// <summary>Routed as POST /api/erp/general-journal/batch.</summary>
    Task<GenJournalBatchDto> CreateBatchAsync(CreateGenJournalBatchDto input);

    /// <summary>Routed as GET /api/erp/general-journal/lines/{batchId}.</summary>
    Task<ListResultDto<GenJournalLineDto>> GetLinesAsync(Guid batchId);

    /// <summary>Routed as POST /api/erp/general-journal/line.</summary>
    Task<GenJournalLineDto> CreateLineAsync(CreateGenJournalLineDto input);

    /// <summary>Routed as DELETE /api/erp/general-journal/line/{lineId}.</summary>
    Task DeleteLineAsync(Guid lineId);

    /// <summary>
    /// Posts every line of the batch. Refused unless each document number balances to zero.
    /// Routed as POST /api/erp/general-journal/run-posting/{batchId}.
    /// </summary>
    Task<GenJournalPostingResultDto> RunPostingAsync(Guid batchId);
}
