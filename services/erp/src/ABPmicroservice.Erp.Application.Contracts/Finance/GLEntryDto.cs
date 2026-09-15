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
}

public class GetGLEntryListInput : PagedAndSortedResultRequestDto
{
    public Guid? GLAccountId { get; set; }
    public string DocumentNo { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
