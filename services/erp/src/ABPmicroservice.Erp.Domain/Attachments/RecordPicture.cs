using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Attachments;

/// <summary>
/// The picture of a record, shown as its avatar on the card: a customer's logo, an item's photo,
/// an employee's portrait. A record has at most one. It is kept apart from the record's
/// attachments so it never shows up in their list and never flows onto posted documents.
/// </summary>
public class RecordPicture : CompanyEntity
{
    /// <summary>The table's registry name, e.g. "Customer".</summary>
    public string EntityName { get; private set; }

    public Guid RecordId { get; private set; }

    /// <summary>The content type taken from the image's own bytes, never the uploader's claim.</summary>
    public string ContentType { get; private set; }

    public long Size { get; private set; }

    public byte[] Content { get; private set; }

    protected RecordPicture() { }

    public RecordPicture(Guid id, string entityName, Guid recordId, string contentType, byte[] content)
        : base(id)
    {
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        RecordId = recordId;
        Replace(contentType, content);
    }

    public void Replace(string contentType, byte[] content)
    {
        ContentType = Check.NotNullOrWhiteSpace(contentType, nameof(contentType), ErpDomainConsts.MaxContentTypeLength);
        Content = Check.NotNull(content, nameof(content));
        Size = content.Length;
    }
}

/// <summary>
/// Which raster image a file is, read from its first bytes. SVG is deliberately not accepted:
/// it can carry script, and the picture is shown inline on the card.
/// </summary>
public static class RecordPictureFormats
{
    /// <summary>The image's content type, or null when the bytes are not a PNG, JPEG, GIF or WebP.</summary>
    public static string Detect(byte[] content)
    {
        if (content == null || content.Length < 12)
        {
            return null;
        }

        if (content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47
            && content[4] == 0x0D && content[5] == 0x0A && content[6] == 0x1A && content[7] == 0x0A)
        {
            return "image/png";
        }

        if (content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (content[0] == 'G' && content[1] == 'I' && content[2] == 'F' && content[3] == '8'
            && (content[4] == '7' || content[4] == '9') && content[5] == 'a')
        {
            return "image/gif";
        }

        if (content[0] == 'R' && content[1] == 'I' && content[2] == 'F' && content[3] == 'F'
            && content[8] == 'W' && content[9] == 'E' && content[10] == 'B' && content[11] == 'P')
        {
            return "image/webp";
        }

        return null;
    }
}

public class RecordPictureManager : DomainService
{
    private readonly IRepository<RecordPicture, Guid> _pictures;
    private readonly DocumentAttachmentManager _attachments;

    public RecordPictureManager(IRepository<RecordPicture, Guid> pictures, DocumentAttachmentManager attachments)
    {
        _pictures = pictures;
        _attachments = attachments;
    }

    public async Task<RecordPicture> FindAsync(string entityName, Guid recordId)
    {
        var name = _attachments.TableNameOf(entityName);
        return await _pictures.FirstOrDefaultAsync(p => p.EntityName == name && p.RecordId == recordId);
    }

    /// <summary>Sets the record's picture, replacing the one it had.</summary>
    public async Task<RecordPicture> SetAsync(string entityName, Guid recordId, byte[] content)
    {
        if (content == null || content.Length == 0)
        {
            throw new BusinessException(ErpErrorCodes.Attachments.FileEmpty);
        }

        if (content.Length > ErpDomainConsts.MaxRecordPictureBytes)
        {
            throw TooLarge();
        }

        var contentType = RecordPictureFormats.Detect(content)
            ?? throw new BusinessException(ErpErrorCodes.Attachments.PictureNotAnImage);

        var name = await _attachments.CheckRecordAsync(entityName, recordId);
        var picture = await _pictures.FirstOrDefaultAsync(p => p.EntityName == name && p.RecordId == recordId);
        if (picture == null)
        {
            picture = new RecordPicture(GuidGenerator.Create(), name, recordId, contentType, content);
            await _pictures.InsertAsync(picture, autoSave: true);
        }
        else
        {
            picture.Replace(contentType, content);
            await _pictures.UpdateAsync(picture, autoSave: true);
        }

        return picture;
    }

    public async Task RemoveAsync(string entityName, Guid recordId)
    {
        var picture = await FindAsync(entityName, recordId);
        if (picture != null)
        {
            await _pictures.DeleteAsync(picture, autoSave: true);
        }
    }

    public static BusinessException TooLarge()
    {
        return new BusinessException(ErpErrorCodes.Attachments.PictureTooLarge)
            .WithData("maxSize", ErpDomainConsts.MaxRecordPictureBytes / (1024 * 1024) + " MB");
    }
}
