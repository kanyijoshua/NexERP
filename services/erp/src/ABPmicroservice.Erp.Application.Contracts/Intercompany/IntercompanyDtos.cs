using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Intercompany;

public class ICPartnerDto : CodeTableDto
{
    public Guid PartnerCompanyId { get; set; }
    public string CustomerNo { get; set; }
    public string VendorNo { get; set; }
    public string DefaultGLAccountNo { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateICPartnerDto : CreateUpdateCodeTableDto
{
    public Guid PartnerCompanyId { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VendorNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string DefaultGLAccountNo { get; set; }

    public bool Blocked { get; set; }
}

public interface IICPartnerAppService : ICrudAppService<ICPartnerDto, Guid, GetCodeTableListInput, CreateUpdateICPartnerDto> { }

public class ICOutboxTransactionDto : FullAuditedEntityDto<Guid>
{
    public long TransactionNo { get; set; }
    public string PartnerCode { get; set; }
    public ICSourceType SourceType { get; set; }
    public string DocumentNo { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal Amount { get; set; }
    public ICOutboxStatus Status { get; set; }
    public DateTime? SentDate { get; set; }
}

public class GetICTransactionListInput : ErpPagedListInput
{
    public string PartnerCode { get; set; }
    public int? Status { get; set; }
}

public class ICSendResultDto
{
    public int SentCount { get; set; }
}

public interface IICOutboxTransactionAppService : IReadOnlyAppService<ICOutboxTransactionDto, Guid, GetICTransactionListInput>
{
    Task<ICSendResultDto> SendAsync(Guid id);

    Task<ICSendResultDto> SendAllAsync();

    Task CancelAsync(Guid id);
}

public class ICInboxTransactionDto : FullAuditedEntityDto<Guid>
{
    public long TransactionNo { get; set; }
    public string PartnerCode { get; set; }
    public ICSourceType SourceType { get; set; }
    public ICDocumentType DocumentType { get; set; }
    public string PartnerDocumentNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public ICInboxStatus Status { get; set; }
    public string CreatedDocumentNo { get; set; }
}

public class ICInboxLineDto : EntityDto<Guid>
{
    public Guid ICInboxTransactionId { get; set; }
    public int LineNo { get; set; }
    public ICLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public decimal LineDiscountPct { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class GetICInboxLineListInput : ErpPagedListInput
{
    public Guid? ICInboxTransactionId { get; set; }
}

public class AcceptICInboxInput
{
    public DateTime? PostingDate { get; set; }
}

public class ICAcceptResultDto
{
    public string DocumentNo { get; set; }
}

public interface IICInboxTransactionAppService : IReadOnlyAppService<ICInboxTransactionDto, Guid, GetICTransactionListInput>
{
    Task<ICAcceptResultDto> AcceptAsync(Guid id, AcceptICInboxInput input);

    Task RejectAsync(Guid id);
}

public interface IICInboxLineAppService : IReadOnlyAppService<ICInboxLineDto, Guid, GetICInboxLineListInput> { }
