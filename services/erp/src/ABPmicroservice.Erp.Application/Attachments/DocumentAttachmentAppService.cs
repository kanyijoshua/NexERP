using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Attachments;

/// <summary>
/// Files attached to records. An attachment says something about the record it hangs on, so every
/// call also needs the permission that lets the caller read that record: holding the attachments
/// permission alone opens nothing.
/// </summary>
[Authorize(ErpPermissions.Attachments.Default)]
public class DocumentAttachmentAppService : ErpAppService, IDocumentAttachmentAppService
{
    private readonly DocumentAttachmentManager _manager;
    private readonly IRepository<DocumentAttachment, Guid> _repository;

    public DocumentAttachmentAppService(DocumentAttachmentManager manager, IRepository<DocumentAttachment, Guid> repository)
    {
        _manager = manager;
        _repository = repository;
    }

    public async Task<ListResultDto<DocumentAttachmentDto>> GetListAsync(GetDocumentAttachmentListInput input)
    {
        await CheckRecordAccessAsync(input.EntityName);

        var attachments = await _manager.GetListAsync(input.EntityName, input.RecordId);
        return new ListResultDto<DocumentAttachmentDto>(
            ObjectMapper.Map<List<DocumentAttachment>, List<DocumentAttachmentDto>>(attachments)
        );
    }

    [Authorize(ErpPermissions.Attachments.Create)]
    public async Task<DocumentAttachmentDto> UploadAsync(UploadDocumentAttachmentInput input)
    {
        await CheckRecordAccessAsync(input.EntityName);

        // Refused on the encoded length, before the file is decoded into memory.
        if ((input.ContentBase64?.Length ?? 0) / 4L * 3 > ErpDomainConsts.MaxAttachmentBytes)
        {
            throw DocumentAttachmentManager.TooLarge();
        }

        byte[] content;
        try
        {
            content = Convert.FromBase64String(input.ContentBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new BusinessException(ErpErrorCodes.Attachments.FileEmpty);
        }

        var attachment = await _manager.AttachAsync(
            input.EntityName,
            input.RecordId,
            input.FileName,
            input.ContentType,
            content,
            CurrentUser.Id,
            CurrentUser.UserName,
            input.LineNo,
            input.DocumentFlowPurchase,
            input.DocumentFlowSales
        );

        return Map(attachment);
    }

    public async Task<IRemoteStreamContent> GetDownloadAsync(Guid id)
    {
        var attachment = await _repository.GetAsync(id);
        await CheckRecordAccessAsync(attachment.EntityName);

        // Always sent as a download, never rendered: the stored content type is what the uploader
        // claimed, and a browser must not be invited to run it in this site's origin.
        return new RemoteStreamContent(
            new MemoryStream(await _manager.GetContentAsync(id)),
            attachment.FullFileName,
            "application/octet-stream"
        );
    }

    [Authorize(ErpPermissions.Attachments.Create)]
    public async Task<DocumentAttachmentDto> UpdateAsync(Guid id, UpdateDocumentAttachmentDto input)
    {
        var attachment = await _repository.GetAsync(id);
        await CheckRecordAccessAsync(attachment.EntityName);

        attachment.Rename(input.FileName);
        attachment.SetDocumentFlow(input.DocumentFlowPurchase, input.DocumentFlowSales);

        await _repository.UpdateAsync(attachment, autoSave: true);
        return Map(attachment);
    }

    [Authorize(ErpPermissions.Attachments.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var attachment = await _repository.GetAsync(id);
        await CheckRecordAccessAsync(attachment.EntityName);

        await _manager.DeleteAsync(attachment);
    }

    /// <summary>The caller may read the table the attachment hangs on; a table with no permission entry is closed.</summary>
    private async Task CheckRecordAccessAsync(string entityName)
    {
        var permission = ErpEntityPermissions.Find(entityName);
        if (permission == null)
        {
            throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGranted);
        }

        await AuthorizationService.CheckAsync(permission);
    }

    private DocumentAttachmentDto Map(DocumentAttachment attachment)
    {
        return ObjectMapper.Map<DocumentAttachment, DocumentAttachmentDto>(attachment);
    }
}
