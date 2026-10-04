using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Attachments;

/// <summary>
/// A file attached to a record: a vendor's contract, the scan behind a purchase invoice, an
/// employee's certificate.
/// <para>
/// The record is addressed by the table's registry name and the
/// record's id, with "No." kept beside it so a list reads without a join. The file itself lives
/// in <see cref="DocumentAttachmentContent"/>, so listing attachments never loads their bytes.
/// </para>
/// </summary>
public class DocumentAttachment : CompanyEntity
{
    /// <summary>The table the record belongs to, as the entity registry names it, e.g. "Vendor".</summary>
    public string EntityName { get; private set; }

    public Guid RecordId { get; private set; }

    /// <summary>The record's "No.", for display.</summary>
    public string No { get; private set; }

    /// <summary>The document line the file belongs to; zero for the record itself.</summary>
    public int LineNo { get; private set; }

    /// <summary>Sequence of the attachment on its record.</summary>
    public int AttachmentNo { get; private set; }

    public DateTime AttachedDate { get; private set; }

    public Guid? AttachedBy { get; private set; }

    /// <summary>Who attached it.</summary>
    public string AttachedByUserName { get; private set; }

    /// <summary>The name without its extension.</summary>
    public string FileName { get; private set; }

    public string FileExtension { get; private set; }

    public DocumentAttachmentFileType FileType { get; private set; }

    public string ContentType { get; private set; }

    /// <summary>Size of the file in bytes.</summary>
    public long Size { get; private set; }

    /// <summary>Carried onto the posted purchase document when the document is posted.</summary>
    public bool DocumentFlowPurchase { get; private set; }

    /// <summary>Carried onto the posted sales document when the document is posted.</summary>
    public bool DocumentFlowSales { get; private set; }

    protected DocumentAttachment() { }

    public DocumentAttachment(
        Guid id,
        string entityName,
        Guid recordId,
        string no,
        int lineNo,
        int attachmentNo,
        string fileName,
        string contentType,
        long size,
        DateTime attachedDate,
        Guid? attachedBy,
        string attachedByUserName
    )
        : base(id)
    {
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        RecordId = recordId;
        No = Check.Length(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        AttachmentNo = attachmentNo;
        Size = size;
        AttachedDate = attachedDate;
        AttachedBy = attachedBy;
        AttachedByUserName = Check.Length(attachedByUserName, nameof(attachedByUserName), ErpDomainConsts.MaxUserNameLength);
        ContentType = Check.Length(contentType, nameof(contentType), ErpDomainConsts.MaxContentTypeLength);
        SetFileName(fileName);
    }

    /// <summary>Takes a full file name; the extension decides the file type.</summary>
    public void SetFileName(string fileName)
    {
        var trimmed = Path.GetFileName(Check.NotNullOrWhiteSpace(fileName, nameof(fileName)).Trim());
        var extension = Path.GetExtension(trimmed).TrimStart('.').ToLowerInvariant();
        var name = Path.GetFileNameWithoutExtension(trimmed);

        FileName = Check.NotNullOrWhiteSpace(name, nameof(fileName), ErpDomainConsts.MaxDescriptionLength);
        FileExtension = Check.Length(extension, nameof(fileName), ErpDomainConsts.MaxFileExtensionLength);
        FileType = DocumentAttachmentFileTypes.Of(extension);
    }

    /// <summary>Keeps the extension: renaming a file must not change what it is.</summary>
    public void Rename(string fileNameWithoutExtension)
    {
        FileName = Check.NotNullOrWhiteSpace(fileNameWithoutExtension?.Trim(), nameof(fileNameWithoutExtension), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetDocumentFlow(bool purchase, bool sales)
    {
        DocumentFlowPurchase = purchase;
        DocumentFlowSales = sales;
    }

    /// <summary>The name a download is saved under.</summary>
    public string FullFileName => FileExtension.IsNullOrEmpty() ? FileName : $"{FileName}.{FileExtension}";
}

/// <summary>The bytes of a <see cref="DocumentAttachment"/>, keyed by the attachment's id.</summary>
public class DocumentAttachmentContent : CompanyBasicEntity
{
    public byte[] Content { get; private set; }

    protected DocumentAttachmentContent() { }

    public DocumentAttachmentContent(Guid attachmentId, byte[] content)
        : base(attachmentId)
    {
        Content = Check.NotNull(content, nameof(content));
    }
}

/// <summary>Which file type an extension is, and which extensions are never accepted.</summary>
public static class DocumentAttachmentFileTypes
{
    private static readonly Dictionary<string, DocumentAttachmentFileType> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jpg"] = DocumentAttachmentFileType.Image,
        ["jpeg"] = DocumentAttachmentFileType.Image,
        ["png"] = DocumentAttachmentFileType.Image,
        ["gif"] = DocumentAttachmentFileType.Image,
        ["bmp"] = DocumentAttachmentFileType.Image,
        ["tif"] = DocumentAttachmentFileType.Image,
        ["tiff"] = DocumentAttachmentFileType.Image,
        ["pdf"] = DocumentAttachmentFileType.Pdf,
        ["doc"] = DocumentAttachmentFileType.Word,
        ["docx"] = DocumentAttachmentFileType.Word,
        ["rtf"] = DocumentAttachmentFileType.Word,
        ["xls"] = DocumentAttachmentFileType.Excel,
        ["xlsx"] = DocumentAttachmentFileType.Excel,
        ["csv"] = DocumentAttachmentFileType.Excel,
        ["ppt"] = DocumentAttachmentFileType.PowerPoint,
        ["pptx"] = DocumentAttachmentFileType.PowerPoint,
        ["msg"] = DocumentAttachmentFileType.Email,
        ["eml"] = DocumentAttachmentFileType.Email,
        ["xml"] = DocumentAttachmentFileType.Xml,
    };

    /// <summary>Programs and scripts: a download of one of these runs on whoever opens it.</summary>
    private static readonly HashSet<string> Blocked = new(StringComparer.OrdinalIgnoreCase)
    {
        "exe", "dll", "com", "scr", "msi", "bat", "cmd", "ps1", "vbs", "vbe", "js", "jse", "wsf", "wsh", "hta", "jar", "lnk",
        "reg", "cpl", "sh", "html", "htm", "svg",
    };

    public static DocumentAttachmentFileType Of(string extension) =>
        ByExtension.GetValueOrDefault(extension ?? string.Empty, DocumentAttachmentFileType.Other);

    public static bool IsBlocked(string extension) => Blocked.Contains(extension ?? string.Empty);
}

/// <summary>
/// Attaches files to records, hands them back, and carries them from a document to the posted
/// document it becomes.
/// </summary>
public class DocumentAttachmentManager : DomainService
{
    private static readonly MethodInfo FindRecordMethod = typeof(DocumentAttachmentManager).GetMethod(
        nameof(FindRecordAsync),
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    private readonly IRepository<DocumentAttachment, Guid> _attachments;
    private readonly IRepository<DocumentAttachmentContent, Guid> _contents;
    private readonly ErpEntityRegistry _registry;

    public DocumentAttachmentManager(
        IRepository<DocumentAttachment, Guid> attachments,
        IRepository<DocumentAttachmentContent, Guid> contents,
        ErpEntityRegistry registry
    )
    {
        _attachments = attachments;
        _contents = contents;
        _registry = registry;
    }

    /// <summary>The attachments of a record, oldest first.</summary>
    public async Task<List<DocumentAttachment>> GetListAsync(string entityName, Guid recordId)
    {
        var name = _registry.Get(entityName).Name;
        return (await _attachments.GetListAsync(a => a.EntityName == name && a.RecordId == recordId))
            .OrderBy(a => a.AttachmentNo)
            .ToList();
    }

    public async Task<DocumentAttachment> AttachAsync(
        string entityName,
        Guid recordId,
        string fileName,
        string contentType,
        byte[] content,
        Guid? userId,
        string userName,
        int lineNo = 0,
        bool documentFlowPurchase = false,
        bool documentFlowSales = false
    )
    {
        Check.NotNull(content, nameof(content));

        if (content.Length == 0)
        {
            throw new BusinessException(ErpErrorCodes.Attachments.FileEmpty);
        }

        if (content.Length > ErpDomainConsts.MaxAttachmentBytes)
        {
            throw TooLarge();
        }

        var extension = Path.GetExtension(fileName ?? string.Empty).TrimStart('.');
        if (DocumentAttachmentFileTypes.IsBlocked(extension))
        {
            throw new BusinessException(ErpErrorCodes.Attachments.FileTypeNotAllowed).WithData("extension", extension);
        }

        var definition = _registry.Get(entityName);
        var record = await FindAsync(definition, recordId)
            ?? throw new BusinessException(ErpErrorCodes.Attachments.RecordNotFound).WithData("entityName", definition.Name);

        var existing = await _attachments.GetListAsync(a => a.EntityName == definition.Name && a.RecordId == recordId);

        var attachment = new DocumentAttachment(
            GuidGenerator.Create(),
            definition.Name,
            recordId,
            NoOf(definition, record),
            lineNo,
            existing.Count == 0 ? 1 : existing.Max(a => a.AttachmentNo) + 1,
            fileName,
            contentType,
            content.Length,
            Clock.Now,
            userId,
            userName
        );
        attachment.SetDocumentFlow(documentFlowPurchase, documentFlowSales);

        await _attachments.InsertAsync(attachment, autoSave: true);
        await _contents.InsertAsync(new DocumentAttachmentContent(attachment.Id, content), autoSave: true);

        return attachment;
    }

    public async Task<byte[]> GetContentAsync(Guid attachmentId)
    {
        return (await _contents.GetAsync(attachmentId)).Content;
    }

    public async Task DeleteAsync(DocumentAttachment attachment)
    {
        await _contents.DeleteAsync(c => c.Id == attachment.Id, autoSave: true);
        await _attachments.DeleteAsync(attachment, autoSave: true);
    }

    /// <summary>
    /// Copies the attachments of a document that are marked to flow onto the posted document it
    /// became. The originals stay where they are.
    /// </summary>
    public async Task FlowToPostedDocumentAsync(
        string entityName,
        Guid recordId,
        string postedEntityName,
        Guid postedRecordId,
        string postedNo,
        bool purchase
    )
    {
        var sourceName = _registry.Get(entityName).Name;
        var targetName = _registry.Get(postedEntityName).Name;

        var flowing = (await _attachments.GetListAsync(a => a.EntityName == sourceName && a.RecordId == recordId))
            .Where(a => purchase ? a.DocumentFlowPurchase : a.DocumentFlowSales)
            .OrderBy(a => a.AttachmentNo)
            .ToList();

        var next = 1;
        foreach (var source in flowing)
        {
            var copy = new DocumentAttachment(
                GuidGenerator.Create(),
                targetName,
                postedRecordId,
                postedNo,
                source.LineNo,
                next++,
                source.FullFileName,
                source.ContentType,
                source.Size,
                source.AttachedDate,
                source.AttachedBy,
                source.AttachedByUserName
            );
            copy.SetDocumentFlow(source.DocumentFlowPurchase, source.DocumentFlowSales);

            await _attachments.InsertAsync(copy, autoSave: true);
            await _contents.InsertAsync(new DocumentAttachmentContent(copy.Id, await GetContentAsync(source.Id)), autoSave: true);
        }
    }

    public static BusinessException TooLarge()
    {
        return new BusinessException(ErpErrorCodes.Attachments.FileTooLarge)
            .WithData("maxSize", ErpDomainConsts.MaxAttachmentBytes / (1024 * 1024) + " MB");
    }

    private Task<object> FindAsync(ErpEntityDefinition definition, Guid recordId)
    {
        if (!typeof(IEntity<Guid>).IsAssignableFrom(definition.EntityType))
        {
            return Task.FromResult<object>(null);
        }

        return (Task<object>)FindRecordMethod.MakeGenericMethod(definition.EntityType).Invoke(this, [recordId]);
    }

    private async Task<object> FindRecordAsync<TEntity>(Guid id)
        where TEntity : class, IEntity<Guid>
    {
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<TEntity, Guid>>().FindAsync(id);
    }

    /// <summary>The record's "No.", or its code or name where the table is keyed by one of those.</summary>
    private static string NoOf(ErpEntityDefinition definition, object record)
    {
        foreach (var name in new[] { "No", "Code", "Name" })
        {
            if (definition.FindField(name)?.GetValue(record) is string value && !value.IsNullOrWhiteSpace())
            {
                return value.Length > ErpDomainConsts.MaxDocumentNoLength ? value[..ErpDomainConsts.MaxDocumentNoLength] : value;
            }
        }

        return null;
    }
}
