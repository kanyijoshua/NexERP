using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.Attachments;

public class DocumentAttachmentDto : EntityDto<Guid>
{
    public string EntityName { get; set; }
    public Guid RecordId { get; set; }
    public string No { get; set; }
    public int LineNo { get; set; }
    public int AttachmentNo { get; set; }
    public DateTime AttachedDate { get; set; }
    public string AttachedByUserName { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public DocumentAttachmentFileType FileType { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public bool DocumentFlowPurchase { get; set; }
    public bool DocumentFlowSales { get; set; }
}

public class GetDocumentAttachmentListInput
{
    /// <summary>The table the record belongs to, e.g. "Vendor" or "PurchaseHeader".</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public Guid RecordId { get; set; }
}

public class UploadDocumentAttachmentInput : GetDocumentAttachmentListInput
{
    /// <summary>The file name with its extension.</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string FileName { get; set; }

    [StringLength(ErpDomainConsts.MaxContentTypeLength)]
    public string ContentType { get; set; }

    /// <summary>The file, base64 encoded.</summary>
    [Required]
    public string ContentBase64 { get; set; }

    public int LineNo { get; set; }

    public bool DocumentFlowPurchase { get; set; }

    public bool DocumentFlowSales { get; set; }
}

public class UpdateDocumentAttachmentDto
{
    /// <summary>The name without its extension; the extension never changes.</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string FileName { get; set; }

    public bool DocumentFlowPurchase { get; set; }

    public bool DocumentFlowSales { get; set; }
}

/// <summary>
/// Files attached to records. Reading the attachments
/// of a record needs the permission to read that record as well as the attachments permission.
/// </summary>
public interface IDocumentAttachmentAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/document-attachment.</summary>
    Task<ListResultDto<DocumentAttachmentDto>> GetListAsync(GetDocumentAttachmentListInput input);

    /// <summary>Routed as POST /api/erp/document-attachment/upload.</summary>
    Task<DocumentAttachmentDto> UploadAsync(UploadDocumentAttachmentInput input);

    /// <summary>Routed as GET /api/erp/document-attachment/{id}/download.</summary>
    Task<IRemoteStreamContent> DownloadAsync(Guid id);

    /// <summary>Routed as PUT /api/erp/document-attachment/{id}.</summary>
    Task<DocumentAttachmentDto> UpdateAsync(Guid id, UpdateDocumentAttachmentDto input);

    /// <summary>Routed as DELETE /api/erp/document-attachment/{id}.</summary>
    Task DeleteAsync(Guid id);
}
