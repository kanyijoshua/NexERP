using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Emailing;

public class EmailTemplateDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public EmailDocumentType DocumentType { get; set; }
    public string SubjectTemplate { get; set; }
    public string BodyHtmlTemplate { get; set; }
    public bool IsDefault { get; set; }
    public bool Active { get; set; }
}

public class CreateUpdateEmailTemplateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public EmailDocumentType DocumentType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEmailSubjectLength)]
    public string SubjectTemplate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEmailBodyLength)]
    public string BodyHtmlTemplate { get; set; }

    public bool IsDefault { get; set; } = true;
    public bool Active { get; set; } = true;
}

public class GetEmailTemplateListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public EmailDocumentType? DocumentType { get; set; }
    public bool? Active { get; set; }
}

public class SentEmailLogDto : FullAuditedEntityDto<Guid>
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public string PartyNo { get; set; }
    public string PartyName { get; set; }
    public string RecipientEmail { get; set; }
    public string CcEmail { get; set; }
    public string Subject { get; set; }
    public string BodyHtml { get; set; }
    public string TemplateCode { get; set; }
    public int AttachmentCount { get; set; }
    public EmailDeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? SentByUserId { get; set; }
    public string ErrorMessage { get; set; }
}

public class GetSentEmailLogListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public EmailDocumentType? DocumentType { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public EmailDeliveryStatus? Status { get; set; }
}

public class PreviewDocumentEmailInput
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string TemplateCode { get; set; }
}

public class DocumentEmailPreviewDto
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public string PartyNo { get; set; }
    public string PartyName { get; set; }
    public string RecipientEmail { get; set; }
    public string TemplateCode { get; set; }
    public string Subject { get; set; }
    public string BodyHtml { get; set; }
    public List<DocumentEmailAttachmentDto> Attachments { get; set; } = [];
}

public class DocumentEmailAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
}

public class SendDocumentEmailInput
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailRecipientsLength)]
    public string RecipientEmail { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailRecipientsLength)]
    public string CcEmail { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailSubjectLength)]
    public string CustomSubject { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailBodyLength)]
    public string CustomBodyHtml { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TemplateCode { get; set; }

    /// <summary>Files attached to the document to send with the email.</summary>
    public List<Guid> AttachmentIds { get; set; } = [];
}

public interface IEmailTemplateAppService
    : ICrudAppService<
        EmailTemplateDto,
        Guid,
        GetEmailTemplateListInput,
        CreateUpdateEmailTemplateDto,
        CreateUpdateEmailTemplateDto
    >
{
    Task<List<string>> GetPlaceholdersAsync();
}

public interface IDocumentEmailAppService : IReadOnlyAppService<SentEmailLogDto, Guid, GetSentEmailLogListInput>
{
    Task<DocumentEmailPreviewDto> PreviewAsync(PreviewDocumentEmailInput input);

    Task<SentEmailLogDto> SendAsync(SendDocumentEmailInput input);
}
