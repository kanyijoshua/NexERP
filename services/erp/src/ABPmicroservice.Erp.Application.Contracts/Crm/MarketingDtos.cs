using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Crm;

public class SegmentDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public int ContactCount { get; set; }
}

public class CreateUpdateSegmentDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetSegmentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

/// <summary>The contacts to add: every criterion given must match; blank criteria match all.</summary>
public class PickSegmentContactsInput
{
    [StringLength(50)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersonCode { get; set; }

    public ContactType? Type { get; set; }
    public bool OnlyWithEmail { get; set; } = true;
}

public class ApplySegmentInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CampaignNo { get; set; }
}

public class SegmentResultDto
{
    public int Count { get; set; }
}

public interface ISegmentAppService : ICrudAppService<SegmentDto, Guid, GetSegmentListInput, CreateUpdateSegmentDto>
{
    Task<SegmentResultDto> PickContactsAsync(Guid id, PickSegmentContactsInput input);

    Task<SegmentResultDto> ApplyToCampaignAsync(Guid id, ApplySegmentInput input);
}

public class SegmentLineDto : EntityDto<Guid>
{
    public Guid SegmentId { get; set; }
    public string ContactNo { get; set; }
    public string ContactName { get; set; }
    public string Email { get; set; }
}

public class GetSegmentLineListInput : ErpPagedListInput
{
    public Guid? SegmentId { get; set; }
}

public interface ISegmentLineAppService : IReadOnlyAppService<SegmentLineDto, Guid, GetSegmentLineListInput>
{
    Task DeleteAsync(Guid id);
}

public class CampaignMailingEntryDto : EntityDto<Guid>
{
    public string CampaignNo { get; set; }
    public string ContactNo { get; set; }
    public string ContactName { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public DateTime SentAt { get; set; }
    public MailingStatus Status { get; set; }
    public string ErrorMessage { get; set; }
}

public class GetCampaignMailingListInput : ErpPagedListInput
{
    public string CampaignNo { get; set; }
}

public class SendMailingInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CampaignNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Subject { get; set; }

    /// <summary>HTML; {{Name}} is replaced by the contact's name.</summary>
    [Required]
    [StringLength(20000)]
    public string BodyHtml { get; set; }
}

public class MailingResultDto
{
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int WithoutEmail { get; set; }
}

public interface ICampaignMailingAppService : IReadOnlyAppService<CampaignMailingEntryDto, Guid, GetCampaignMailingListInput>
{
    Task<MailingResultDto> SendAsync(SendMailingInput input);
}
